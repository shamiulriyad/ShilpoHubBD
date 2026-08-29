namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Primary function of a <see cref="Warehouse"/>.</summary>
public enum WarehouseType
{
    Distribution,
    Fulfillment,
    ColdStorage,
    CrossDock,
    Returns,
    Hub,
}
