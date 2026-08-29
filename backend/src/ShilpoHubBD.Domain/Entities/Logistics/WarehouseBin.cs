namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>A discrete storage location within a <see cref="Warehouse"/> (optionally inside a zone).</summary>
public class WarehouseBin
{
    public Guid Id { get; set; }

    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public Guid? WarehouseZoneId { get; set; }
    public WarehouseZone? Zone { get; set; }

    /// <summary>Location code, e.g. <c>A-12-03</c>. Unique within the warehouse.</summary>
    public string Code { get; set; } = string.Empty;
    public string? Label { get; set; }
    public WarehouseBinType Type { get; set; }

    /// <summary>Max units this bin can hold (0 = unspecified / unlimited).</summary>
    public int CapacityUnits { get; set; }

    /// <summary>Current on-hand units in this bin. Maintained on write.</summary>
    public int OccupiedUnits { get; set; }

    public bool IsPickable { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
