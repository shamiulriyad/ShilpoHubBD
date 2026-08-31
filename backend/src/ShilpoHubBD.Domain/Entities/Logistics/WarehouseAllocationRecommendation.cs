using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>
/// A rule-based ranking of a partner's warehouses for holding / routing a given consignment,
/// produced by the pluggable smart-warehouse-allocation provider. Scores free capacity, proximity,
/// cold-chain fit and utilisation, weighted by the chosen objective. No real model.
/// </summary>
public class WarehouseAllocationRecommendation
{
    public Guid Id { get; set; }

    public Guid LogisticsPartnerProfileId { get; set; }
    public LogisticsPartnerProfile Profile { get; set; } = null!;

    public Guid GeneratedByUserId { get; set; }
    public User GeneratedBy { get; set; } = null!;

    public WarehouseAllocationObjective Objective { get; set; }

    // ---- Consignment context -----------------------------------------
    public string? Sku { get; set; }
    public int? Quantity { get; set; }
    public bool RequireColdChain { get; set; }

    public Guid? DestinationDistrictId { get; set; }
    public District? DestinationDistrict { get; set; }

    public Guid? ShipmentId { get; set; }
    public Shipment? Shipment { get; set; }

    public string Method { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public AiLogisticsConfidence Confidence { get; set; }

    public Guid? RecommendedWarehouseId { get; set; }
    public Warehouse? RecommendedWarehouse { get; set; }
    public string? RecommendedWarehouseCode { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<WarehouseAllocationOption> Options { get; set; } = new List<WarehouseAllocationOption>();
}
