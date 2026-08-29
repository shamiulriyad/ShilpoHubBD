using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>
/// A storage / fulfilment facility operated by a <see cref="LogisticsPartnerProfile"/>. Holds
/// <see cref="Zones"/> and <see cref="Bins"/> that stock is stored in; individual holdings are
/// tracked as <see cref="WarehouseStockItem"/>s with a <see cref="WarehouseStockMovement"/> ledger.
/// </summary>
public class Warehouse
{
    public Guid Id { get; set; }

    /// <summary>Human reference, format <c>WH-yyyyMM-#####</c>. Unique.</summary>
    public string Code { get; set; } = string.Empty;

    public Guid LogisticsPartnerProfileId { get; set; }
    public LogisticsPartnerProfile Profile { get; set; } = null!;

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public WarehouseType Type { get; set; } = WarehouseType.Distribution;
    public WarehouseStatus Status { get; set; } = WarehouseStatus.Active;

    public string AddressLine { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public Guid? DistrictId { get; set; }
    public District? District { get; set; }
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public string? ContactPersonName { get; set; }
    public string? ContactPhone { get; set; }

    /// <summary>Nominal storage capacity in stock units (0 = unspecified).</summary>
    public int TotalCapacityUnits { get; set; }

    /// <summary>Sum of on-hand quantity across all stock items. Maintained on write.</summary>
    public int UsedCapacityUnits { get; set; }

    public bool HasColdChain { get; set; }
    public bool HandlesHazardous { get; set; }
    public bool HandlesReturns { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<WarehouseZone> Zones { get; set; } = new List<WarehouseZone>();
    public ICollection<WarehouseBin> Bins { get; set; } = new List<WarehouseBin>();
}
