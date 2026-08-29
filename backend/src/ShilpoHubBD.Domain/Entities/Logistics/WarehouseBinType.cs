namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Physical form of a <see cref="WarehouseBin"/> storage location.</summary>
public enum WarehouseBinType
{
    Shelf,
    Rack,
    Pallet,
    Floor,
    Bulk,
    Bin,
    ColdUnit,
}
