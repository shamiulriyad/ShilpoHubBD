using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.ManufacturingPartnership;

public class ManufacturingPartnership
{
    public Guid Id { get; set; }

    public Guid BusinessPartnerId { get; set; }
    public User BusinessPartner { get; set; } = null!;

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    public string ProductRequirements { get; set; } = string.Empty;
    public string ManufacturingSpecifications { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal? TargetUnitPrice { get; set; }

    public DateTime TimelineStartDate { get; set; }
    public DateTime TimelineEndDate { get; set; }

    public PartnershipStatus Status { get; set; } = PartnershipStatus.Requested;
    public string? ProducerResponseNotes { get; set; }
    public DateTime? RespondedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ManufacturingMilestone> Milestones { get; set; } = new List<ManufacturingMilestone>();
    public ICollection<PartnershipStatusEvent> StatusHistory { get; set; } = new List<PartnershipStatusEvent>();
}
