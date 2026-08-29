namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>A functional area inside a <see cref="Warehouse"/> (receiving, storage, dispatch, ...).</summary>
public class WarehouseZone
{
    public Guid Id { get; set; }

    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>Short code, unique within the warehouse.</summary>
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public WarehouseZoneType Type { get; set; }

    public bool IsColdChain { get; set; }
    public int CapacityUnits { get; set; }
    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
