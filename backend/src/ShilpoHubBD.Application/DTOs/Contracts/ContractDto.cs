using ShilpoHubBD.Domain.Entities.Contracts;

namespace ShilpoHubBD.Application.DTOs.Contracts;

public class ContractDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;

    public Guid BusinessPartnerId { get; set; }
    public string BusinessPartnerName { get; set; } = string.Empty;

    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Terms { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsExpired { get; set; }
    public bool AutoRenew { get; set; }
    public int? RenewalTermMonths { get; set; }

    public ContractStatus Status { get; set; }

    public Guid? PreviousContractId { get; set; }

    public decimal ContractValue { get; set; }

    public List<ContractItemDto> Items { get; set; } = new();
    public List<ContractDeliveryScheduleDto> DeliverySchedules { get; set; } = new();
    public List<ContractDocumentDto> Documents { get; set; } = new();
    public List<ContractStatusEventDto> StatusHistory { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
