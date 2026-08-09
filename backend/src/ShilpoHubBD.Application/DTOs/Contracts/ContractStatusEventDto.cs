using ShilpoHubBD.Domain.Entities.Contracts;

namespace ShilpoHubBD.Application.DTOs.Contracts;

public class ContractStatusEventDto
{
    public ContractStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
