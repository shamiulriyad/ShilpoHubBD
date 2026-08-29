namespace ShilpoHubBD.Application.DTOs.Logistics;

public class ReceiveStockRequest
{
    public Guid WarehouseId { get; set; }
    public Guid? WarehouseBinId { get; set; }

    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? UnitOfMeasure { get; set; }

    public int Quantity { get; set; }

    public Guid? ProductId { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal? UnitValue { get; set; }

    /// <summary>When set, adds to this existing stock item instead of matching / creating one.</summary>
    public Guid? StockItemId { get; set; }

    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Reason { get; set; }
    public string? Note { get; set; }
    public DateTime? OccurredAt { get; set; }
}

public class IssueStockRequest
{
    public int Quantity { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Reason { get; set; }
    public string? Note { get; set; }
    public DateTime? OccurredAt { get; set; }
}

public class TransferStockRequest
{
    public Guid ToBinId { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public DateTime? OccurredAt { get; set; }
}

public class AdjustStockRequest
{
    /// <summary>New absolute on-hand quantity. Provide this OR <see cref="Delta"/>.</summary>
    public int? NewQuantityOnHand { get; set; }

    /// <summary>Signed change to on-hand quantity. Provide this OR <see cref="NewQuantityOnHand"/>.</summary>
    public int? Delta { get; set; }

    /// <summary>Adjustment, StockCount, Damage or Disposal. Defaults to Adjustment.</summary>
    public string? MovementType { get; set; }

    /// <summary>Optional new status for the stock item.</summary>
    public string? Status { get; set; }

    public string Reason { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime? OccurredAt { get; set; }
}

public class ReserveStockRequest
{
    public int Quantity { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Note { get; set; }
    public DateTime? OccurredAt { get; set; }
}

public class WarehouseStockQueryParameters
{
    public Guid? WarehouseId { get; set; }
    public Guid? WarehouseBinId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string? Status { get; set; }
    public string? Sku { get; set; }
    public bool? LowStock { get; set; }
    public bool? ExpiringSoon { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class WarehouseStockMovementQueryParameters
{
    public Guid? WarehouseId { get; set; }
    public Guid? WarehouseStockItemId { get; set; }
    public string? Type { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
