using ShilpoHubBD.Domain.Entities.Contracts;

namespace ShilpoHubBD.Application.DTOs.Contracts;

public class ContractDeliveryScheduleDto
{
    public Guid Id { get; set; }
    public DateTime ScheduledDate { get; set; }
    public int Quantity { get; set; }
    public ContractDeliveryStatus Status { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public string? Notes { get; set; }
}
