namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Kind of change recorded on a <see cref="WarehouseStockMovement"/> ledger row.</summary>
public enum WarehouseStockMovementType
{
    Inbound,
    Outbound,
    TransferOut,
    TransferIn,
    Adjustment,
    Reserve,
    ReleaseReservation,
    Damage,
    Disposal,
    StockCount,
}
