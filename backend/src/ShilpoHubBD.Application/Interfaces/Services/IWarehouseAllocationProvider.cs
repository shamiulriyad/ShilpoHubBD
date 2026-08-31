using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>
/// Ranks a partner's warehouses for holding / routing a consignment. The default implementation
/// scores free capacity, proximity, cold-chain fit and utilisation and weights them by the chosen
/// objective; swap for a real optimiser later without touching the service or controller.
/// </summary>
public interface IWarehouseAllocationProvider
{
    string ProviderName { get; }

    WarehouseAllocationResult Recommend(WarehouseAllocationInput input);
}

public record WarehouseAllocationInput
{
    public string Objective { get; init; } = "balanced";
    public bool RequireColdChain { get; init; }
    public int? Quantity { get; init; }
    public Guid? DestinationDistrictId { get; init; }
    public IReadOnlyList<WarehouseCandidate> Candidates { get; init; } = Array.Empty<WarehouseCandidate>();
}

public record WarehouseCandidate(
    Guid WarehouseId,
    string Code,
    string Name,
    Guid? DistrictId,
    bool HasColdChain,
    int TotalCapacityUnits,
    int UsedCapacityUnits,
    string Status);

public record WarehouseAllocationResult(
    string Method,
    string Summary,
    AiLogisticsConfidence Confidence,
    Guid? RecommendedWarehouseId,
    IReadOnlyList<WarehouseAllocationOptionResult> Options);

public record WarehouseAllocationOptionResult(
    Guid WarehouseId,
    string Code,
    string Name,
    int Rank,
    double Score,
    double ProjectedUtilizationPercent,
    bool SameDistrictAsDestination,
    string Rationale);
