namespace ShilpoHubBD.Domain.Entities.Reviews;

public enum ProductModerationEventType
{
    RepeatedComplaintDetected = 0,
    WarningIssued = 1,
    HighRiskFlagged = 2,

    /// <summary>An admin banned the product from a moderation case (Part 3). Never set by AI.</summary>
    ProductBanned = 3,
}

/// <summary>
/// One row per moderation decision on a product — the audit trail ("moderation history") behind
/// <see cref="ProductModerationState"/>. Its name intentionally ends in "Event" so the generic
/// notification-generation pass in <c>ShilpoHubDbContext.Notifications.cs</c> (which skips every
/// <c>*Event</c> entity, same as <c>MonitoringFlagEvent</c>/<c>InvestmentStatusEvent</c>) never turns a
/// history row into a producer-facing notification by itself — that's what <see cref="ProducerModerationWarning"/>
/// is for.
/// </summary>
public class ProductModerationEvent
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }
    public ProductModerationState ProductModerationState { get; set; } = null!;

    /// <summary>The review that triggered this evaluation.</summary>
    public Guid ReviewId { get; set; }

    public ProductModerationEventType Type { get; set; }
    public ReviewComplaintType ComplaintType { get; set; }
    public ReviewSeverity Severity { get; set; }

    /// <summary>The AI's (or the rule-based fallback's) explanation for this decision.</summary>
    public string Reason { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public bool IsAiGenerated { get; set; }

    /// <summary>Ids of the historical reviews actually retrieved and shown to the AI for this comparison.</summary>
    public List<Guid> SimilarReviewIds { get; set; } = new();

    public DateTime CreatedAt { get; set; }
}
