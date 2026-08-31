using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>
/// Tracks a producer / village / district / product / organisation against a compliance framework
/// (authenticity standards, safeguarding obligations, grant conditions …). Holds a checklist of
/// requirements and a rolled-up status + score.
/// </summary>
public class ComplianceRecord
{
    public Guid Id { get; set; }

    public ComplianceEntityType EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string EntityLabel { get; set; } = string.Empty;

    public string Framework { get; set; } = string.Empty;

    public ComplianceStatus Status { get; set; } = ComplianceStatus.NotStarted;

    /// <summary>0–100, derived from mandatory requirement completion.</summary>
    public decimal OverallScorePercent { get; set; }

    public DateTime PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }

    public DateTime? LastReviewedAt { get; set; }
    public DateTime? NextReviewDue { get; set; }

    public Guid? ReviewerUserId { get; set; }
    public User? Reviewer { get; set; }

    public string? Notes { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ComplianceRequirement> Requirements { get; set; } = new List<ComplianceRequirement>();
}
