namespace ShilpoHubBD.Application.DTOs.Logistics;

public class WarehouseStockItemDto
{
    public Guid Id { get; set; }

    public Guid WarehouseId { get; set; }
    public string? WarehouseCode { get; set; }
    public string? WarehouseName { get; set; }

    public Guid? WarehouseBinId { get; set; }
    public string? BinCode { get; set; }

    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string? OwnerName { get; set; }

    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = "unit";

    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int QuantityAvailable { get; set; }

    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? UnitValue { get; set; }

    public DateTime? ReceivedAt { get; set; }
    public DateTime? LastMovementAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<WarehouseStockMovementDto> Movements { get; set; } = new();
}

public class WarehouseStockItemListItemDto
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public string? BinCode { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = "unit";
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int QuantityAvailable { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ProductName { get; set; }
    public DateTime? LastMovementAt { get; set; }
}

public class WarehouseStockMovementDto
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? WarehouseStockItemId { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int QuantityOnHandAfter { get; set; }
    public Guid? FromBinId { get; set; }
    public Guid? ToBinId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Reason { get; set; }
    public string? Note { get; set; }
    public Guid? PerformedByUserId { get; set; }
    public string? PerformedByName { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
