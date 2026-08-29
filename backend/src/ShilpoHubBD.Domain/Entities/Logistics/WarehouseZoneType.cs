namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Functional area within a <see cref="Warehouse"/>.</summary>
public enum WarehouseZoneType
{
    Receiving,
    Storage,
    Picking,
    Packing,
    Dispatch,
    Returns,
    ColdStorage,
    Quarantine,
    Staging,
}
