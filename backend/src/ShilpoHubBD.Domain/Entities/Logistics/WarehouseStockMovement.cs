using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>
/// An append-only ledger row recording one change to warehouse stock: a receipt, an issue, a
/// bin-to-bin transfer leg, a reservation, an adjustment or a stock count.
/// </summary>
public class WarehouseStockMovement
{
    public Guid Id { get; set; }

    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public Guid? WarehouseStockItemId { get; set; }
    public WarehouseStockItem? StockItem { get; set; }

    public WarehouseStockMovementType Type { get; set; }

    /// <summary>Always positive; the sign of the effect is implied by <see cref="Type"/>.</summary>
    public int Quantity { get; set; }

    public int QuantityOnHandAfter { get; set; }

    public Guid? FromBinId { get; set; }
    public Guid? ToBinId { get; set; }

    public string Sku { get; set; } = string.Empty;

    /// <summary>Free label of what triggered this, e.g. <c>Shipment</c>, <c>PickupRequest</c>, <c>Order</c>.</summary>
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }

    public string? Reason { get; set; }
    public string? Note { get; set; }

    public Guid? PerformedByUserId { get; set; }
    public User? PerformedBy { get; set; }

    public DateTime OccurredAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
