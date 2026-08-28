using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>
/// A concern raised by the Government / NGO monitoring tools — a fraud-risk pattern, a suspected fake
/// product, review abuse, a QR-verification anomaly or a compliance gap. Flags are produced by
/// rule-based scans or raised manually, then triaged to resolution.
/// </summary>
public class MonitoringFlag
{
    public Guid Id { get; set; }

    public MonitoringFlagType FlagType { get; set; }
    public MonitoringFlagSeverity Severity { get; set; }
    public MonitoringFlagStatus Status { get; set; } = MonitoringFlagStatus.Open;
    public MonitoringFlagSource Source { get; set; }

    public MonitoringSubjectType SubjectType { get; set; }
    public Guid? SubjectId { get; set; }
    public string SubjectLabel { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>Signals that triggered the flag, serialised as JSON.</summary>
    public string? EvidenceJson { get; set; }

    /// <summary>0–100 heuristic risk score.</summary>
    public decimal RiskScore { get; set; }

    /// <summary>Stable key used to avoid raising duplicate open flags for the same finding.</summary>
    public string DedupeKey { get; set; } = string.Empty;

    public DateTime DetectedAt { get; set; }

    public Guid? AssignedToUserId { get; set; }
    public User? AssignedTo { get; set; }

    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public User? ResolvedBy { get; set; }
    public string? ResolutionNotes { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<MonitoringFlagEvent> Events { get; set; } = new List<MonitoringFlagEvent>();
}
