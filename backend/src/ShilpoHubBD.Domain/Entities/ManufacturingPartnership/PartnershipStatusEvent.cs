namespace ShilpoHubBD.Domain.Entities.ManufacturingPartnership;

public class PartnershipStatusEvent
{
    public Guid Id { get; set; }

    public Guid PartnershipId { get; set; }
    public ManufacturingPartnership Partnership { get; set; } = null!;

    public PartnershipStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
