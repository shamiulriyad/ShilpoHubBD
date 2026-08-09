namespace ShilpoHubBD.Domain.Entities.Contracts;

public class ContractDeliverySchedule
{
    public Guid Id { get; set; }

    public Guid ContractId { get; set; }
    public Contract Contract { get; set; } = null!;

    public DateTime ScheduledDate { get; set; }
    public int Quantity { get; set; }
    public ContractDeliveryStatus Status { get; set; } = ContractDeliveryStatus.Pending;
    public DateTime? ActualDeliveryDate { get; set; }
    public string? Notes { get; set; }
}
