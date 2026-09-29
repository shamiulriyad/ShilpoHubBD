using System.Text.RegularExpressions;
using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Infrastructure.ReviewModeration;

/// <summary>
/// Deterministic keyword/rating-based fallback used when Gemini is unavailable or fails, so review moderation
/// never breaks and never "invents" an AI verdict — every field here is derived transparently from the review's
/// own rating and comment text. Mirrors the rule-based fallback pattern used across this project (e.g.
/// <c>DummyProductIntelligenceAIProvider</c>, <c>RuleBasedSentimentAnalysisProvider</c>).
/// </summary>
public class RuleBasedReviewModerationProvider : IReviewModerationAIProvider
{
    private static readonly Regex WordSplitter = new(@"[^a-zA-Z']+", RegexOptions.Compiled);

    private static readonly HashSet<string> QualityWords = new(StringComparer.OrdinalIgnoreCase)
        { "broken", "damaged", "defective", "poor", "cheap", "torn", "faded", "flimsy", "quality", "worn" };

    private static readonly HashSet<string> ShippingWords = new(StringComparer.OrdinalIgnoreCase)
        { "late", "delay", "delayed", "delivery", "courier", "shipping", "shipment", "packaging", "package" };

    private static readonly HashSet<string> CounterfeitWords = new(StringComparer.OrdinalIgnoreCase)
        { "fake", "counterfeit", "duplicate", "replica", "notauthentic", "notoriginal", "scam" };

    private static readonly HashSet<string> ServiceWords = new(StringComparer.OrdinalIgnoreCase)
        { "rude", "unresponsive", "support", "seller", "producer", "behavior", "attitude", "ignored" };

    private static readonly HashSet<string> PricingWords = new(StringComparer.OrdinalIgnoreCase)
        { "expensive", "overpriced", "price", "pricey", "costly" };

    private static readonly HashSet<string> GeneralNegativeWords = new(StringComparer.OrdinalIgnoreCase)
        { "bad", "terrible", "awful", "worst", "disappointed", "horrible", "unhappy", "refund", "waste" };

    public Task<ReviewModerationResultDto> AnalyzeAsync(ReviewModerationContext context, CancellationToken cancellationToken)
    {
        var words = WordSplitter.Split(context.Comment ?? string.Empty).Where(w => w.Length > 0).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var qualityHits = words.Count(QualityWords.Contains);
        var shippingHits = words.Count(ShippingWords.Contains);
        var counterfeitHits = words.Count(CounterfeitWords.Contains);
        var serviceHits = words.Count(ServiceWords.Contains);
        var pricingHits = words.Count(PricingWords.Contains);
        var generalNegativeHits = words.Count(GeneralNegativeWords.Contains);

        var totalHits = qualityHits + shippingHits + counterfeitHits + serviceHits + pricingHits + generalNegativeHits;
        var isNegative = context.Rating <= 2 || totalHits > 0;

        if (!isNegative)
        {
            return Task.FromResult(new ReviewModerationResultDto
            {
                IsNegative = false,
                IsProductRelated = true,
                ComplaintType = ReviewComplaintType.None,
                Severity = ReviewSeverity.None,
                IssueSummary = "No significant complaint detected.",
                Confidence = 0.4,
                IsAiGenerated = false,
            });
        }

        var (complaintType, isProductRelated) = (counterfeitHits, qualityHits, shippingHits, serviceHits, pricingHits) switch
        {
            ( > 0, _, _, _, _) => (ReviewComplaintType.Counterfeit, true),
            (_, > 0, _, _, _) => (ReviewComplaintType.Quality, true),
            (_, _, > 0, 0, _) => (ReviewComplaintType.Shipping, false),
            (_, _, _, > 0, _) => (ReviewComplaintType.CustomerService, false),
            (_, _, _, _, > 0) => (ReviewComplaintType.Pricing, true),
            _ => (ReviewComplaintType.Other, true),
        };

        var severity = counterfeitHits > 0 || context.Rating <= 1
            ? ReviewSeverity.High
            : context.Rating == 2
                ? ReviewSeverity.Medium
                : ReviewSeverity.Low;
        if (counterfeitHits > 0 && context.Rating <= 1)
        {
            severity = ReviewSeverity.Critical;
        }

        return Task.FromResult(new ReviewModerationResultDto
        {
            IsNegative = true,
            IsProductRelated = isProductRelated,
            ComplaintType = complaintType,
            Severity = severity,
            IssueSummary = $"Automated rule-based scan flagged a {complaintType} concern from a {context.Rating}-star review.",
            Confidence = Math.Min(0.4 + 0.1 * totalHits, 0.7),
            IsAiGenerated = false,
        });
    }
}
