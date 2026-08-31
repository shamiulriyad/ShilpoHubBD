namespace ShilpoHubBD.Application.DTOs.Governance;

public class MonitoringFlagDto
{
    public Guid Id { get; set; }
    public string FlagType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;

    public string SubjectType { get; set; } = string.Empty;
    public Guid? SubjectId { get; set; }
    public string SubjectLabel { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? EvidenceJson { get; set; }
    public decimal RiskScore { get; set; }
    public DateTime DetectedAt { get; set; }

    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }

    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedByName { get; set; }
    public string? ResolutionNotes { get; set; }

    public Guid CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<MonitoringFlagEventDto> Events { get; set; } = new();
}

public class MonitoringFlagEventDto
{
    public string Type { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string? FromStatus { get; set; }
    public string? ToStatus { get; set; }
    public Guid ActorUserId { get; set; }
    public string? ActorName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MonitoringFlagListItemDto
{
    public Guid Id { get; set; }
    public string FlagType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SubjectType { get; set; } = string.Empty;
    public Guid? SubjectId { get; set; }
    public string SubjectLabel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal RiskScore { get; set; }
    public DateTime DetectedAt { get; set; }
    public string? AssignedToName { get; set; }
}
