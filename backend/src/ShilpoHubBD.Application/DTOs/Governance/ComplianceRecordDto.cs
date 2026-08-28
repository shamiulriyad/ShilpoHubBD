namespace ShilpoHubBD.Application.DTOs.Governance;

public class ComplianceRecordDto
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string EntityLabel { get; set; } = string.Empty;
    public string Framework { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal OverallScorePercent { get; set; }

    public DateTime PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public DateTime? LastReviewedAt { get; set; }
    public DateTime? NextReviewDue { get; set; }

    public Guid? ReviewerUserId { get; set; }
    public string? ReviewerName { get; set; }
    public string? Notes { get; set; }

    public Guid CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<ComplianceRequirementDto> Requirements { get; set; } = new();
}

public class ComplianceRequirementDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsMandatory { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Evidence { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int DisplayOrder { get; set; }
}

public class ComplianceRecordListItemDto
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string EntityLabel { get; set; } = string.Empty;
    public string Framework { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal OverallScorePercent { get; set; }
    public DateTime? NextReviewDue { get; set; }
    public DateTime UpdatedAt { get; set; }
}
