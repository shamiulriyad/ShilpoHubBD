using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IAiLogisticsRepository
{
    // ---- Signal gathering -----------------------------------------------
    Task<Shipment?> GetShipmentAsync(Guid shipmentId, CancellationToken cancellationToken);

    Task<LaneDeliveryStats> GetLaneStatsAsync(
        Guid profileId, Guid? originDistrictId, Guid? destinationDistrictId, DateTime sinceUtc,
        CancellationToken cancellationToken);

    Task<PartnerDeliveryStats> GetPartnerDeliveryStatsAsync(
        Guid profileId, DateTime sinceUtc, CancellationToken cancellationToken);

    Task<List<DemandDailyCount>> GetDailyDemandSeriesAsync(
        Guid profileId, DemandForecastScope scope, Guid? scopeId, string metric,
        DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken);

    Task<DeliveryRoute?> GetRouteWithStopsAsync(Guid routeId, CancellationToken cancellationToken);

    Task<List<Warehouse>> GetCandidateWarehousesAsync(Guid profileId, CancellationToken cancellationToken);

    // ---- Reference checks ---------------------------------------------
    Task<bool> ShipmentBelongsToProfileAsync(Guid shipmentId, Guid profileId, CancellationToken cancellationToken);

    Task<bool> RouteBelongsToProfileAsync(Guid routeId, Guid profileId, CancellationToken cancellationToken);

    Task<bool> WarehouseBelongsToProfileAsync(Guid warehouseId, Guid profileId, CancellationToken cancellationToken);

    Task<bool> DistrictExistsAsync(Guid districtId, CancellationToken cancellationToken);

    Task<string?> GetDistrictNameAsync(Guid districtId, CancellationToken cancellationToken);

    Task<string?> GetWarehouseLabelAsync(Guid warehouseId, CancellationToken cancellationToken);

    // ---- Delivery predictions ------------------------------------
    Task AddDeliveryPredictionAsync(DeliveryPrediction prediction, CancellationToken cancellationToken);
    void RemoveDeliveryPrediction(DeliveryPrediction prediction);
    Task<DeliveryPrediction?> GetDeliveryPredictionByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<DeliveryPrediction> Items, int TotalCount)> GetDeliveryPredictionsPagedAsync(
        Guid? profileId, AiLogisticsQueryParameters query, CancellationToken cancellationToken);

    // ---- Route optimization runs -------------------------------
    Task AddRouteOptimizationRunAsync(RouteOptimizationRun run, CancellationToken cancellationToken);
    void RemoveRouteOptimizationRun(RouteOptimizationRun run);
    Task<RouteOptimizationRun?> GetRouteOptimizationRunByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<RouteOptimizationRun> Items, int TotalCount)> GetRouteOptimizationRunsPagedAsync(
        Guid? profileId, AiLogisticsQueryParameters query, CancellationToken cancellationToken);

    // ---- Demand forecasts -------------------------------------
    Task AddDemandForecastAsync(DemandForecast forecast, CancellationToken cancellationToken);
    void RemoveDemandForecast(DemandForecast forecast);
    Task<DemandForecast?> GetDemandForecastByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<DemandForecast> Items, int TotalCount)> GetDemandForecastsPagedAsync(
        Guid? profileId, AiLogisticsQueryParameters query, CancellationToken cancellationToken);

    // ---- Warehouse allocation recommendations --------------
    Task AddWarehouseAllocationAsync(WarehouseAllocationRecommendation recommendation, CancellationToken cancellationToken);
    void RemoveWarehouseAllocation(WarehouseAllocationRecommendation recommendation);
    Task<WarehouseAllocationRecommendation?> GetWarehouseAllocationByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<WarehouseAllocationRecommendation> Items, int TotalCount)> GetWarehouseAllocationsPagedAsync(
        Guid? profileId, AiLogisticsQueryParameters query, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public record LaneDeliveryStats(
    double? AverageTransitDays, double? OnTimeRate, double? FailureRate, int SampleSize);

public record PartnerDeliveryStats(
    double? OnTimeRate, double? AverageAttempts, double? FailureRate, int SampleSize);

public record DemandDailyCount(DateTime Date, double Value);
