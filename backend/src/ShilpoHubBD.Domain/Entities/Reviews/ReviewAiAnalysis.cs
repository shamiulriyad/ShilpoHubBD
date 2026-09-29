namespace ShilpoHubBD.Domain.Entities.Reviews;

public enum ReviewComplaintType
{
    None = 0,
    Quality,
    Shipping,
    Counterfeit,
    CustomerService,
    Pricing,
    Other,
}

public enum ReviewSeverity
{
    None = 0,
    Low,
    Medium,
    High,
    Critical,
}

/// <summary>
/// AI moderation analysis of a product <see cref="Review"/>, produced once after the review is created.
/// Kept as its own 1:1 record rather than columns on Review: Review is the customer-facing record of what was
/// said, this is the AI's read of it. Deleting/regenerating an analysis never touches the review itself.
/// </summary>
public class ReviewAiAnalysis
{
    public Guid Id { get; set; }

    public Guid ReviewId { get; set; }
    public Review Review { get; set; } = null!;

    public bool IsNegative { get; set; }
    public bool IsProductRelated { get; set; }
    public ReviewComplaintType ComplaintType { get; set; }
    public ReviewSeverity Severity { get; set; }
    public string IssueSummary { get; set; } = string.Empty;

    /// <summary>0-1. How confident the analysis is in its own verdict.</summary>
    public double Confidence { get; set; }

    /// <summary>False when Gemini was unavailable and the deterministic rule-based fallback produced this instead.</summary>
    public bool IsAiGenerated { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
