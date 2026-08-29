using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>
/// A quantity of one SKU held in a <see cref="Warehouse"/> (and a <see cref="WarehouseBin"/> when
/// binned). May be linked to a marketplace <see cref="Product"/> and to the producer that owns the
/// goods. <see cref="QuantityAvailable"/> is kept in sync as <c>OnHand − Reserved</c>.
/// </summary>
public class WarehouseStockItem
{
    public Guid Id { get; set; }

    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public Guid? WarehouseBinId { get; set; }
    public WarehouseBin? Bin { get; set; }

    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }

    /// <summary>Producer / owner of the goods, when known.</summary>
    public Guid? OwnerUserId { get; set; }
    public User? Owner { get; set; }

    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = "unit";

    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int QuantityAvailable { get; set; }

    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public WarehouseStockItemStatus Status { get; set; } = WarehouseStockItemStatus.Available;

    public decimal? UnitValue { get; set; }

    public DateTime? ReceivedAt { get; set; }
    public DateTime? LastMovementAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
