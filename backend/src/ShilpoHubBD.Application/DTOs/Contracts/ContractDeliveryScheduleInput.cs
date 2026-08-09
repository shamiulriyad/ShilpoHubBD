namespace ShilpoHubBD.Application.DTOs.Contracts;

public class ContractDeliveryScheduleInput
{
    public DateTime ScheduledDate { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}
