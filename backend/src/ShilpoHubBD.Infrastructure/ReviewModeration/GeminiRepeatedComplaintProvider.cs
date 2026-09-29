using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Reviews;
using ShilpoHubBD.Infrastructure.Options;

namespace ShilpoHubBD.Infrastructure.ReviewModeration;

/// <summary>
/// Real repeated-complaint comparison via the Gemini API, reusing the same <see cref="GeminiOptions"/>/API key
/// as every other Gemini-backed provider (no second AI configuration, no new client). Gemini is given ONLY the
/// new review and the historical reviews the caller actually retrieved from Postgres — it is asked to classify
/// the comparison, never to invent a historical review that wasn't supplied. Any missing API key, HTTP failure,
/// timeout or malformed response falls back to <see cref="RuleBasedRepeatedComplaintProvider"/>.
/// </summary>
public class GeminiRepeatedComplaintProvider : IRepeatedComplaintAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly RuleBasedRepeatedComplaintProvider _fallback;
    private readonly ILogger<GeminiRepeatedComplaintProvider> _logger;

    public GeminiRepeatedComplaintProvider(
        HttpClient httpClient, IOptions<GeminiOptions> options, RuleBasedRepeatedComplaintProvider fallback, ILogger<GeminiRepeatedComplaintProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _fallback = fallback;
        _logger = logger;
    }

    public async Task<RepeatedComplaintResultDto> CompareAsync(RepeatedComplaintContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey) || context.HistoricalReviews.Count == 0)
        {
            return await _fallback.CompareAsync(context, cancellationToken);
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

            var parsed = JsonSerializer.Deserialize<GeminiComparison>(text, JsonOptions);
            if (parsed is null)
            {
                return await _fallback.CompareAsync(context, cancellationToken);
            }

            return new RepeatedComplaintResultDto
            {
                IsRepeatedComplaint = parsed.IsRepeatedComplaint,
                ComplaintType = ParseEnum(parsed.ComplaintType, context.NewComplaintType),
                Severity = ParseEnum(parsed.Severity, context.NewSeverity),
                Reason = parsed.Reason ?? string.Empty,
                Confidence = Math.Clamp(parsed.Confidence, 0, 1),
                IsAiGenerated = true,
            };
        }
        catch (Exception exc) when (exc is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(exc, "Gemini repeated-complaint call failed; falling back to the rule-based provider.");
            return await _fallback.CompareAsync(context, cancellationToken);
        }
    }

    private static TEnum ParseEnum<TEnum>(string? value, TEnum fallback) where TEnum : struct, Enum
        => !string.IsNullOrWhiteSpace(value) && Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) ? parsed : fallback;

    private static string BuildPrompt(RepeatedComplaintContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You compare a new customer complaint about a Bangladeshi handicraft product against a list of " +
            "PREVIOUSLY RETRIEVED historical reviews for the same product. Use ONLY the reviews listed below — never assume " +
            "or invent any other historical review.");
        sb.AppendLine("RULES:");
        sb.AppendLine("1. isRepeatedComplaint: true only if at least one listed historical review describes the same underlying " +
            "problem as the new review (semantically, not just matching words).");
        sb.AppendLine("2. complaintType: one of Quality, Shipping, Counterfeit, CustomerService, Pricing, Other — the category shared by the repeated complaints.");
        sb.AppendLine("3. severity: one of Low, Medium, High, Critical.");
        sb.AppendLine("4. reason: one short factual sentence citing what makes them similar, grounded only in the texts given.");
        sb.AppendLine("5. confidence: a number between 0 and 1.");
        sb.AppendLine("6. Reply with JSON only.");
        sb.AppendLine();
        sb.AppendLine($"Product: {context.ProductName}");
        sb.AppendLine($"New review (complaint type: {context.NewComplaintType}, severity: {context.NewSeverity}): \"{context.NewReviewText}\"");
        sb.AppendLine();
        sb.AppendLine("Historical reviews actually retrieved from the system:");
        foreach (var h in context.HistoricalReviews)
        {
            sb.AppendLine($"- [{(h.SameProduct ? "same product" : "other product")}, complaint type: {h.ComplaintType}, severity: {h.Severity}] \"{h.Text}\"");
        }

        sb.AppendLine();
        sb.AppendLine("Respond with strict JSON only, matching this shape: " +
            "{ \"isRepeatedComplaint\": boolean, \"complaintType\": string, \"severity\": string, \"reason\": string, \"confidence\": number }.");

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

    private class GeminiComparison
    {
        public bool IsRepeatedComplaint { get; set; }
        public string? ComplaintType { get; set; }
        public string? Severity { get; set; }
        public string? Reason { get; set; }
        public double Confidence { get; set; }
    }
}
