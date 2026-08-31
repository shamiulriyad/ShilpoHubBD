namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>One ranked candidate warehouse on a <see cref="WarehouseAllocationRecommendation"/>.</summary>
public class WarehouseAllocationOption
{
    public Guid Id { get; set; }

    public Guid WarehouseAllocationRecommendationId { get; set; }
    public WarehouseAllocationRecommendation Recommendation { get; set; } = null!;

    /// <summary>The <see cref="Warehouse"/> scored. Not a FK — snapshot ranking.</summary>
    public Guid WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;

    public int Rank { get; set; }
    public double Score { get; set; }
    public double ProjectedUtilizationPercent { get; set; }
    public bool SameDistrictAsDestination { get; set; }
    public string Rationale { get; set; } = string.Empty;
}
