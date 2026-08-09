namespace ShilpoHubBD.Application.DTOs.Contracts;

public class CreateContractRequest
{
    public Guid ProducerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Terms { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool AutoRenew { get; set; }
    public int? RenewalTermMonths { get; set; }
    public List<ContractItemInput> Items { get; set; } = new();
    public List<ContractDeliveryScheduleInput> DeliverySchedules { get; set; } = new();
}
