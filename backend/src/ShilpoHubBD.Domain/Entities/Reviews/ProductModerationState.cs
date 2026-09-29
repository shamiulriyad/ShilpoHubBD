using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Reviews;

public enum ProductModerationRiskState
{
    Normal = 0,
    Warning = 1,
    HighRisk = 2,
}

/// <summary>
/// Cumulative, product-level moderation tracking. One row per product, updated deterministically by backend
/// logic every time a negative review is evaluated for repeated complaints — Gemini classifies a single
/// review's comparison to history, but never decides the product's risk state itself. Counts only ever grow
/// within this part; this part never bans or restricts a product automatically.
/// </summary>
public class ProductModerationState
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int NegativeComplaintCount { get; set; }
    public int SimilarComplaintCount { get; set; }
    public int HighSeverityComplaintCount { get; set; }
    public int ProducerWarningCount { get; set; }

    public ProductModerationRiskState RiskState { get; set; } = ProductModerationRiskState.Normal;

    public Guid? LastReviewId { get; set; }
    public DateTime? LastEvaluatedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ProductModerationEvent> Events { get; set; } = new List<ProductModerationEvent>();
}
