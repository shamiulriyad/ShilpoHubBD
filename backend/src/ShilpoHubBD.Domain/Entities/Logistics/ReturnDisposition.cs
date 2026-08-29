namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>What happens to a returned item after inspection.</summary>
public enum ReturnDisposition
{
    Pending,
    Restock,
    ReturnToProducer,
    Repair,
    Refurbish,
    Scrap,
    Donate,
}
