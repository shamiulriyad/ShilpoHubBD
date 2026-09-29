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
/// Real review-moderation analysis via the Gemini API, reusing the same <see cref="GeminiOptions"/>/API key as
/// every other Gemini-backed provider in this project (no second AI configuration, no new client). Gemini is
/// given only the review's own text, rating and product name — it never receives other customers' data. Any
/// missing API key, HTTP failure, timeout or malformed response falls back to the deterministic
/// <see cref="RuleBasedReviewModerationProvider"/> so review creation is never affected.
/// </summary>
public class GeminiReviewModerationProvider : IReviewModerationAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly RuleBasedReviewModerationProvider _fallback;
    private readonly ILogger<GeminiReviewModerationProvider> _logger;

    public GeminiReviewModerationProvider(
        HttpClient httpClient, IOptions<GeminiOptions> options, RuleBasedReviewModerationProvider fallback, ILogger<GeminiReviewModerationProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _fallback = fallback;
        _logger = logger;
    }

    public async Task<ReviewModerationResultDto> AnalyzeAsync(ReviewModerationContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(context.Comment))
        {
            return await _fallback.AnalyzeAsync(context, cancellationToken);
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

            var parsed = JsonSerializer.Deserialize<GeminiAnalysis>(text, JsonOptions);
            if (parsed is null)
            {
                return await _fallback.AnalyzeAsync(context, cancellationToken);
            }

            return new ReviewModerationResultDto
            {
                IsNegative = parsed.IsNegative,
                IsProductRelated = parsed.IsProductRelated,
                ComplaintType = ParseEnum(parsed.ComplaintType, ReviewComplaintType.Other),
                Severity = ParseEnum(parsed.Severity, ReviewSeverity.Medium),
                IssueSummary = parsed.IssueSummary ?? string.Empty,
                Confidence = Math.Clamp(parsed.Confidence, 0, 1),
                IsAiGenerated = true,
            };
        }
        catch (Exception exc) when (exc is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(exc, "Gemini review-moderation call failed; falling back to the rule-based provider.");
            return await _fallback.AnalyzeAsync(context, cancellationToken);
        }
    }

    private static TEnum ParseEnum<TEnum>(string? value, TEnum fallback) where TEnum : struct, Enum
        => !string.IsNullOrWhiteSpace(value) && Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) ? parsed : fallback;

    private static string BuildPrompt(ReviewModerationContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You moderate customer reviews for a Bangladeshi handicraft marketplace. Analyze ONLY the review " +
            "text given below; do not assume facts about the product or producer that are not stated in it.");
        sb.AppendLine("RULES:");
        sb.AppendLine("1. isNegative: true if the review expresses dissatisfaction, a complaint, or a problem.");
        sb.AppendLine("2. isProductRelated: true only if the complaint is about the product itself (quality, authenticity, materials, " +
            "workmanship) rather than delivery/courier, customer service, or something unrelated to the item.");
        sb.AppendLine("3. complaintType: one of None, Quality, Shipping, Counterfeit, CustomerService, Pricing, Other. Use None only when isNegative is false.");
        sb.AppendLine("4. severity: one of None, Low, Medium, High, Critical. Use None only when isNegative is false.");
        sb.AppendLine("5. issueSummary: one short factual sentence describing the main issue, grounded only in the review text.");
        sb.AppendLine("6. confidence: a number between 0 and 1 reflecting how clearly the text supports your verdict.");
        sb.AppendLine("7. Reply with JSON only.");
        sb.AppendLine();
        sb.AppendLine($"Product: {context.ProductName}");
        sb.AppendLine($"Star rating given: {context.Rating}/5");
        sb.AppendLine($"Review text: \"{context.Comment}\"");
        sb.AppendLine();
        sb.AppendLine("Respond with strict JSON only, matching this shape: " +
            "{ \"isNegative\": boolean, \"isProductRelated\": boolean, \"complaintType\": string, \"severity\": string, " +
            "\"issueSummary\": string, \"confidence\": number }.");

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

    private class GeminiAnalysis
    {
        public bool IsNegative { get; set; }
        public bool IsProductRelated { get; set; }
        public string? ComplaintType { get; set; }
        public string? Severity { get; set; }
        public string? IssueSummary { get; set; }
        public double Confidence { get; set; }
    }
}
