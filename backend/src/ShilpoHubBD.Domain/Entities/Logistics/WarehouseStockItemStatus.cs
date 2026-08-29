namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Disposition of a <see cref="WarehouseStockItem"/> holding.</summary>
public enum WarehouseStockItemStatus
{
    Available,
    Reserved,
    Damaged,
    Quarantined,
    Expired,
    OnHold,
}
