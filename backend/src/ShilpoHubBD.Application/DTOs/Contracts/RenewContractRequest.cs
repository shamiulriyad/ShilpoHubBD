namespace ShilpoHubBD.Application.DTOs.Contracts;

public class RenewContractRequest
{
    public DateTime NewEndDate { get; set; }
    public List<ContractItemInput>? Items { get; set; }
    public List<ContractDeliveryScheduleInput>? DeliverySchedules { get; set; }
}
