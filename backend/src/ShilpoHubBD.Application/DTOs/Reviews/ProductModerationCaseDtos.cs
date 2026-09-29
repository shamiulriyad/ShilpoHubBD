namespace ShilpoHubBD.Application.DTOs.Reviews;

/// <summary>One row in the admin moderation list — the columns Part 3 asks for, joined live from
/// <c>MonitoringFlag</c> (existing) and <c>ProductModerationState</c> (Part 2), never from a frozen snapshot.</summary>
public class ProductModerationCaseListItemDto
{
    public Guid FlagId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string RiskState { get; set; } = string.Empty;
    public int SimilarComplaintCount { get; set; }
    public int HighSeverityComplaintCount { get; set; }
    public int ProducerWarningCount { get; set; }
    public string ModerationStatus { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
}

/// <summary>A single review shown as evidence (the triggering review, or one of the retrieved similar
/// reviews) — always hydrated fresh from Postgres, never from the flag's frozen text snapshot.</summary>
public class ModerationReviewDto
{
    public Guid ReviewId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ComplaintType { get; set; }
    public string? Severity { get; set; }

    /// <summary>0-1 relevance score from the vector search. Null when not available (Part 2 does not persist
    /// per-item scores; shown honestly as unavailable rather than invented).</summary>
    public double? Similarity { get; set; }
}

/// <summary>One entry in the moderation case's combined history — a <c>ProductModerationEvent</c> (automated),
/// a <c>ProducerModerationWarning</c>, or a <c>MonitoringFlagEvent</c> (an admin action) — merged and sorted.</summary>
public class ModerationHistoryItemDto
{
    /// <summary>ModerationEvent | Warning | AdminAction</summary>
    public string Kind { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string? ActorName { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>The full evidence an admin needs to decide a moderation case — not just the AI's own summary.</summary>
public class ProductModerationCaseDto
{
    public Guid FlagId { get; set; }
    public string FlagStatus { get; set; } = string.Empty;
    public string FlagSeverity { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }

    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string ProductApprovalStatus { get; set; } = string.Empty;

    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string ProducerEmail { get; set; } = string.Empty;

    // Review statistics
    public int TotalReviews { get; set; }
    public int NegativeReviews { get; set; }
    public int SimilarComplaints { get; set; }
    public int HighSeverityComplaints { get; set; }
    public int PreviousWarnings { get; set; }
    public string RiskState { get; set; } = string.Empty;

    public ModerationReviewDto? TriggeringReview { get; set; }
    public List<ModerationReviewDto> SimilarReviews { get; set; } = new();

    // AI analysis
    public string? IssueSummary { get; set; }
    public bool? IsRepeatedComplaint { get; set; }
    public string? AiComplaintType { get; set; }
    public string? AiSeverity { get; set; }
    public double? AiConfidence { get; set; }
    public string? AiExplanation { get; set; }
    public bool? IsAiGenerated { get; set; }

    public List<ModerationHistoryItemDto> History { get; set; } = new();
}

/// <summary>Confirmation payload for the Ban action; <c>Reason</c> is shown to the admin before they confirm
/// and stored on the product's own (existing) rejection-reason field.</summary>
public class BanProductRequest
{
    public string? Reason { get; set; }
}
