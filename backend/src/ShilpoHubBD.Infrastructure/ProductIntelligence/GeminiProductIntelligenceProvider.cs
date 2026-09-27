using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.ProductIntelligence;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Infrastructure.Options;

namespace ShilpoHubBD.Infrastructure.ProductIntelligence;

/// <summary>
/// Real narrative analysis via the Gemini API, reusing the same GeminiOptions/API key as every other
/// Gemini-backed provider in this project (no second AI configuration). Gemini is given ONLY the
/// numbers ProductIntelligenceService already calculated from the database — it interprets them, it
/// never receives raw order/review/product rows and cannot invent a statistic that wasn't computed.
/// Any missing API key, HTTP failure, timeout or malformed response falls back to the deterministic
/// <see cref="DummyProductIntelligenceAIProvider"/> so the feature never breaks.
/// </summary>
public class GeminiProductIntelligenceProvider : IProductIntelligenceAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly DummyProductIntelligenceAIProvider _fallback;
    private readonly ILogger<GeminiProductIntelligenceProvider> _logger;

    public GeminiProductIntelligenceProvider(
        HttpClient httpClient, IOptions<GeminiOptions> options, DummyProductIntelligenceAIProvider fallback, ILogger<GeminiProductIntelligenceProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _fallback = fallback;
        _logger = logger;
    }

    public async Task<ProductIntelligenceAiInsightsDto> GenerateInsightsAsync(ProductIntelligenceAiContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return await _fallback.GenerateInsightsAsync(context, cancellationToken);
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

            var parsed = JsonSerializer.Deserialize<GeminiInsights>(text, JsonOptions);
            if (parsed is null)
            {
                return await _fallback.GenerateInsightsAsync(context, cancellationToken);
            }

            return new ProductIntelligenceAiInsightsDto
            {
                DemandTrend = parsed.DemandTrend ?? string.Empty,
                EstimatedNextPeriodDemand = parsed.EstimatedNextPeriodDemand ?? string.Empty,
                SalesTrendInterpretation = parsed.SalesTrendInterpretation ?? string.Empty,
                InventoryRecommendation = parsed.InventoryRecommendation ?? string.Empty,
                PricingObservation = parsed.PricingObservation ?? string.Empty,
                MarketingOpportunities = parsed.MarketingOpportunities ?? new List<string>(),
                RiskIndicators = parsed.RiskIndicators ?? new List<string>(),
                IsAiGenerated = true,
                GeneratedAt = DateTime.UtcNow,
            };
        }
        catch (Exception exc) when (exc is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(exc, "Gemini product-intelligence call failed; falling back to the rule-based provider.");
            return await _fallback.GenerateInsightsAsync(context, cancellationToken);
        }
    }

    private static string BuildPrompt(ProductIntelligenceAiContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are a business-intelligence analyst for a Bangladeshi handicraft marketplace. " +
            "You ANALYZE the already-computed numbers below; you are not a source of facts and must never invent a " +
            "sales figure, rating, stock level or any other statistic that is not explicitly given.");
        sb.AppendLine("RULES:");
        sb.AppendLine("1. Use ONLY the numbers supplied below. Do not reference external market knowledge, competitor data, or general craft-market trends you were not given.");
        sb.AppendLine("2. Every output field must be phrased as analysis, an estimate, a forecast, or a recommendation — never as a guaranteed outcome. Use words like \"suggests\", \"estimated\", \"likely\", \"consider\".");
        sb.AppendLine("3. If HasSufficientHistory is false, say so plainly and keep every field short and conservative — do not speculate beyond \"not enough data\".");
        sb.AppendLine("4. estimatedNextPeriodDemand must be derived by extrapolating the given per-period units — state the reasoning briefly, do not just assert a number.");
        sb.AppendLine("5. Reply with JSON only.");
        sb.AppendLine();
        sb.AppendLine($"Product: {context.ProductName}" + (string.IsNullOrWhiteSpace(context.CategoryName) ? "" : $" (category: {context.CategoryName})"));
        sb.AppendLine($"Selected range: {context.RangeLabel}");
        sb.AppendLine($"Has sufficient order history: {context.HasSufficientHistory}");
        sb.AppendLine($"Average rating: {context.AverageRating} ({context.TotalReviewCount} reviews)");
        sb.AppendLine($"Current stock: {context.CurrentStock}" + (context.LowStockThreshold.HasValue ? $" (low-stock threshold: {context.LowStockThreshold})" : ""));
        sb.AppendLine($"Revenue growth (recent half vs prior half of the range): {(context.RevenueGrowthPercent.HasValue ? $"{context.RevenueGrowthPercent}%" : "not available")}");
        sb.AppendLine($"Units-sold growth (recent half vs prior half of the range): {(context.UnitsGrowthPercent.HasValue ? $"{context.UnitsGrowthPercent}%" : "not available")}");
        sb.AppendLine("Per-period data (delivered orders only):");
        foreach (var period in context.Periods)
        {
            sb.AppendLine($"- {period.PeriodLabel}: unitsSold={period.UnitsSold} orders={period.OrderCount} revenue={period.Revenue} newReviews={period.NewReviews} wishlistAdds={period.WishlistAdds}");
        }

        sb.AppendLine();
        sb.AppendLine("Respond with strict JSON only, matching this shape: " +
            "{ \"demandTrend\": string, \"estimatedNextPeriodDemand\": string, \"salesTrendInterpretation\": string, " +
            "\"inventoryRecommendation\": string, \"pricingObservation\": string, \"marketingOpportunities\": string[], \"riskIndicators\": string[] }.");

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

    private class GeminiInsights
    {
        public string? DemandTrend { get; set; }
        public string? EstimatedNextPeriodDemand { get; set; }
        public string? SalesTrendInterpretation { get; set; }
        public string? InventoryRecommendation { get; set; }
        public string? PricingObservation { get; set; }
        public List<string>? MarketingOpportunities { get; set; }
        public List<string>? RiskIndicators { get; set; }
    }
}
