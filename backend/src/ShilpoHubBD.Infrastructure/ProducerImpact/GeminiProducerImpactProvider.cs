using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Infrastructure.Options;

namespace ShilpoHubBD.Infrastructure.ProducerImpact;

/// <summary>
/// Real narrative interpretation via the Gemini API, reusing the same GeminiOptions/API key as every
/// other Gemini-backed provider in this project. Gemini is given ONLY the already-computed, already-
/// classified numbers in ProducerImpactAiContext — it never receives raw order/report rows and cannot
/// invent or recalculate a statistic. Any missing API key, HTTP failure, timeout, or malformed response
/// falls back to the deterministic <see cref="RuleBasedProducerImpactProvider"/> so the feature never breaks.
/// </summary>
public class GeminiProducerImpactProvider : IProducerImpactAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly RuleBasedProducerImpactProvider _fallback;
    private readonly ILogger<GeminiProducerImpactProvider> _logger;

    public GeminiProducerImpactProvider(
        HttpClient httpClient, IOptions<GeminiOptions> options, RuleBasedProducerImpactProvider fallback, ILogger<GeminiProducerImpactProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _fallback = fallback;
        _logger = logger;
    }

    public async Task<ProducerImpactNarrativeDto> GenerateNarrativeAsync(ProducerImpactAiContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return await _fallback.GenerateNarrativeAsync(context, cancellationToken);
        }

        try
        {
            var prompt = BuildPrompt(context);
            var request = new HttpRequestMessage(HttpMethod.Post, $"models/{_options.Model}:generateContent")
            {
                Content = new StringContent(JsonSerializer.Serialize(new GeminiRequest
                {
                    Contents = [new GeminiContent { Parts = [new GeminiPart { Text = prompt }] }],
                    GenerationConfig = new GeminiGenerationConfig(),
                }), Encoding.UTF8, "application/json"),
            };
            request.Headers.Add("x-goog-api-key", _options.ApiKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: cancellationToken);
            var text = body?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new JsonException("Gemini response contained no text.");
            }

            var parsed = JsonSerializer.Deserialize<GeminiNarrative>(text, JsonOptions);
            if (parsed?.Findings is null || parsed.Findings.Count == 0)
            {
                return await _fallback.GenerateNarrativeAsync(context, cancellationToken);
            }

            return new ProducerImpactNarrativeDto
            {
                IsAiGenerated = true,
                ProviderName = "Gemini",
                Findings = parsed.Findings
                    .Where(f => !string.IsNullOrWhiteSpace(f.Text) && !string.IsNullOrWhiteSpace(f.Category) && !string.IsNullOrWhiteSpace(f.Kind)
                        && ValidCategories.Contains(f.Category) && ValidKinds.Contains(f.Kind))
                    .Select(f => new ProducerImpactFindingDto { Category = f.Category!, Kind = f.Kind!, Text = f.Text! })
                    .ToList(),
            };
        }
        catch (Exception exc) when (exc is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(exc, "Gemini producer-impact call failed; falling back to the rule-based provider.");
            return await _fallback.GenerateNarrativeAsync(context, cancellationToken);
        }
    }

    private static readonly HashSet<string> ValidCategories = new()
    {
        "MainPerformanceChange", "IdentifiedProblem", "ObservedImprovement", "AreaNeedingAttention", "PossibleSupportArea",
    };
    private static readonly HashSet<string> ValidKinds = new() { "MeasuredFact", "Recommendation" };

    private static string BuildPrompt(ProducerImpactAiContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are interpreting already-verified performance data for a Government/NGO support case in a " +
            "Bangladeshi handicraft marketplace. You ANALYZE the numbers below in plain language; you are not a source " +
            "of numbers and must never invent, recalculate, or adjust any figure that is not explicitly given.");
        sb.AppendLine("RULES:");
        sb.AppendLine("1. Use ONLY the numbers supplied below. Every Status (Improved/Declined/NoSignificantChange/InsufficientData) was already decided by fixed rules — never reclassify a metric or contradict its given Status.");
        sb.AppendLine("2. Findings with kind \"MeasuredFact\" must describe only what the numbers show, with no speculation and no suggestion.");
        sb.AppendLine("3. Findings with kind \"Recommendation\" must be phrased as a hedge — use words like \"consider\", \"may help\", \"could\" — never as a certainty.");
        sb.AppendLine("4. NEVER state or imply that the Government/NGO support CAUSED an improvement. If a metric's Status is \"Improved\", you may only say the improvement was observed \"during\" or \"coinciding with\" the support period, and must explicitly note this is not proof of causation. If MonthsUsedForComparison is less than 2, or a metric's Status is \"InsufficientData\", say plainly that there is not enough data to assess that metric — do not guess.");
        sb.AppendLine("5. category must be one of exactly: MainPerformanceChange, IdentifiedProblem, ObservedImprovement, AreaNeedingAttention, PossibleSupportArea. Categories MainPerformanceChange, IdentifiedProblem, and ObservedImprovement must use kind \"MeasuredFact\". Categories AreaNeedingAttention and PossibleSupportArea must use kind \"Recommendation\".");
        sb.AppendLine("6. Produce 1-3 findings per category. Reply with JSON only.");
        sb.AppendLine();
        sb.AppendLine($"Producer: {context.ProducerName}");
        sb.AppendLine($"Support organization: {context.SupportOrganizationName}");
        sb.AppendLine($"Support type: {context.SupportType}");
        sb.AppendLine($"Support date: {context.SupportDate:yyyy-MM-dd}");
        sb.AppendLine($"Before period: {context.BeforeYear}-{context.BeforeMonth:D2}, After period: {context.AfterYear}-{context.AfterMonth:D2}");
        sb.AppendLine($"MonthsUsedForComparison: {context.MonthsUsedForComparison} (0, 1, or 2 — 2 means both months have a report)");
        sb.AppendLine("Metrics (already classified, do not recompute):");
        foreach (var m in context.Metrics)
        {
            sb.AppendLine($"- {m.MetricType}: before={m.BeforeValue?.ToString() ?? "null"} after={m.AfterValue?.ToString() ?? "null"} " +
                $"changeAbsolute={m.ChangeAbsolute?.ToString() ?? "null"} changePercentage={m.ChangePercentage?.ToString() ?? "null"} status={m.Status}");
        }

        if (context.OverallSalesRank.HasValue)
        {
            sb.AppendLine($"After-period peer positioning: overall sales rank={context.OverallSalesRank}, percentile={context.OverallSalesPercentile}%, " +
                $"category ({context.CategoryName})={(context.CategoryPosition.HasValue ? $"position {context.CategoryPosition}, category average sales {context.CategoryAverageSales}" : "not available")}, " +
                $"district ({context.DistrictName})={(context.DistrictPosition.HasValue ? $"position {context.DistrictPosition}, district average sales {context.DistrictAverageSales}" : "not available")}.");
        }

        sb.AppendLine();
        sb.AppendLine("Respond with strict JSON only, matching this shape: " +
            "{ \"findings\": [ { \"category\": string, \"kind\": string, \"text\": string } ] }.");

        return sb.ToString();
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public List<GeminiContent> Contents { get; set; } = [];

        [JsonPropertyName("generationConfig")]
        public GeminiGenerationConfig? GenerationConfig { get; set; }
    }

    private class GeminiGenerationConfig
    {
        [JsonPropertyName("responseMimeType")]
        public string ResponseMimeType { get; set; } = "application/json";
    }

    private class GeminiContent
    {
        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; } = [];
    }

    private class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    private class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }

    private class GeminiNarrative
    {
        public List<GeminiFinding>? Findings { get; set; }
    }

    private class GeminiFinding
    {
        public string? Category { get; set; }
        public string? Kind { get; set; }
        public string? Text { get; set; }
    }
}
