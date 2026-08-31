namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>What a <see cref="WarehouseAllocationRecommendation"/> optimises for.</summary>
public enum WarehouseAllocationObjective
{
    Balanced,
    Proximity,
    Capacity,
    ColdChain,
    Cost,
}
