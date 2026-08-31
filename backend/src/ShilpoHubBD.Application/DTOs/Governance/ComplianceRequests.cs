namespace ShilpoHubBD.Application.DTOs.Governance;

public class CreateComplianceRecordRequest
{
    /// <summary>Producer, Village, District, Product or Organization.</summary>
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string EntityLabel { get; set; } = string.Empty;

    public string Framework { get; set; } = string.Empty;
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public DateTime? NextReviewDue { get; set; }
    public Guid? ReviewerUserId { get; set; }
    public string? Notes { get; set; }

    public List<UpsertComplianceRequirementRequest> Requirements { get; set; } = new();
}

public class UpdateComplianceRecordRequest
{
    public string? Framework { get; set; }

    /// <summary>NotStarted, InProgress, Compliant, NonCompliant, Waived or Expired. Leave null to auto-derive from requirements.</summary>
    public string? Status { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public DateTime? NextReviewDue { get; set; }
    public Guid? ReviewerUserId { get; set; }
    public string? Notes { get; set; }
    public bool MarkReviewedNow { get; set; }
}

public class UpsertComplianceRequirementRequest
{
    /// <summary>Omit to add a new requirement; supply to update an existing one.</summary>
    public Guid? Id { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsMandatory { get; set; } = true;

    /// <summary>Met, Unmet, Partial or NotApplicable.</summary>
    public string Status { get; set; } = "Unmet";
    public string? Evidence { get; set; }
    public int DisplayOrder { get; set; }
}

public class ComplianceQueryParameters
{
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? Status { get; set; }
    public string? Framework { get; set; }
    public string? Search { get; set; }
    public bool? ReviewDueOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
