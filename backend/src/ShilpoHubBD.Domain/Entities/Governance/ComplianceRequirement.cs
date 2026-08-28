namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>One line item on a <see cref="ComplianceRecord"/>'s checklist.</summary>
public class ComplianceRequirement
{
    public Guid Id { get; set; }

    public Guid ComplianceRecordId { get; set; }
    public ComplianceRecord Record { get; set; } = null!;

    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public bool IsMandatory { get; set; } = true;
    public ComplianceRequirementStatus Status { get; set; } = ComplianceRequirementStatus.Unmet;

    public string? Evidence { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public int DisplayOrder { get; set; }
}
