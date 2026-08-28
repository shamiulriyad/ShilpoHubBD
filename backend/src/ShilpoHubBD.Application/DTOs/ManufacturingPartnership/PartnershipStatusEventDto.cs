using ShilpoHubBD.Domain.Entities.ManufacturingPartnership;

namespace ShilpoHubBD.Application.DTOs.ManufacturingPartnership;

public class PartnershipStatusEventDto
{
    public PartnershipStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
