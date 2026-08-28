namespace ShilpoHubBD.Domain.Entities.Contracts;

public class ContractStatusEvent
{
    public Guid Id { get; set; }

    public Guid ContractId { get; set; }
    public Contract Contract { get; set; } = null!;

    public ContractStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
