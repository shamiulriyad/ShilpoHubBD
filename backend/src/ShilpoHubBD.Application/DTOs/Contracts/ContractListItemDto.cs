using ShilpoHubBD.Domain.Entities.Contracts;

namespace ShilpoHubBD.Application.DTOs.Contracts;

public class ContractListItemDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ProducerName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal ContractValue { get; set; }
    public ContractStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
