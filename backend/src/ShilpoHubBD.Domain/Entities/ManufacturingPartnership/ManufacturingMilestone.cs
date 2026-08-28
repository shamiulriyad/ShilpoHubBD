namespace ShilpoHubBD.Domain.Entities.ManufacturingPartnership;

public class ManufacturingMilestone
{
    public Guid Id { get; set; }

    public Guid PartnershipId { get; set; }
    public ManufacturingPartnership Partnership { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;
    public DateTime? CompletedAt { get; set; }
    public int DisplayOrder { get; set; }
}
