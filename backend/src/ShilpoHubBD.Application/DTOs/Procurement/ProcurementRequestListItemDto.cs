using ShilpoHubBD.Domain.Entities.Procurement;

namespace ShilpoHubBD.Application.DTOs.Procurement;

public class ProcurementRequestListItemDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ProducerName { get; set; } = string.Empty;
    public decimal ItemsTotal { get; set; }
    public DateTime DeliveryDeadline { get; set; }
    public ProcurementStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
