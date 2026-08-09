namespace ShilpoHubBD.Application.DTOs.ManufacturingPartnership;

public class CreatePartnershipRequest
{
    public Guid ProducerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ProductRequirements { get; set; } = string.Empty;
    public string ManufacturingSpecifications { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal? TargetUnitPrice { get; set; }
    public DateTime TimelineStartDate { get; set; }
    public DateTime TimelineEndDate { get; set; }
    public List<MilestoneInput> Milestones { get; set; } = new();
}
