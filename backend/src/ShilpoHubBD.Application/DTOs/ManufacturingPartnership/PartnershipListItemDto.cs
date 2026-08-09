using ShilpoHubBD.Domain.Entities.ManufacturingPartnership;

namespace ShilpoHubBD.Application.DTOs.ManufacturingPartnership;

public class PartnershipListItemDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ProducerName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime TimelineEndDate { get; set; }
    public PartnershipStatus Status { get; set; }
    public int ProgressPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
}
