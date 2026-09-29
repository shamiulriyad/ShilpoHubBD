namespace ShilpoHubBD.Application.Options;

// Configurable thresholds for repeated-complaint product moderation (Part 2). Gemini classifies a single
// comparison; these thresholds — not Gemini — decide the product's risk state. Bound from the
// "ProductModeration" configuration section so they can be tuned without a code change.
public class ProductModerationOptions
{
    /// <summary>How many historical reviews to retrieve (RAG or DB fallback) when checking for repeats.</summary>
    public int SimilarReviewLookback { get; set; } = 20;

    /// <summary>SimilarComplaintCount at/above this moves a product from Normal to Warning.</summary>
    public int WarningThreshold { get; set; } = 3;

    /// <summary>SimilarComplaintCount at/above this moves a product to HighRisk.</summary>
    public int HighRiskSimilarComplaintThreshold { get; set; } = 5;

    /// <summary>HighSeverityComplaintCount at/above this also moves a product to HighRisk, even below the similar-count threshold.</summary>
    public int HighRiskSeverityComplaintThreshold { get; set; } = 2;
}
