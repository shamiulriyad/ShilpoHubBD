using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IAiLogisticsService
{
    // ---- Delivery prediction -------------------------------------------
    Task<DeliveryPredictionDto> PredictDeliveryAsync(
        Guid currentUserId, bool isAdmin, PredictDeliveryRequest request, CancellationToken cancellationToken);

    Task<PagedResult<DeliveryPredictionListItemDto>> GetDeliveryPredictionsAsync(
        Guid currentUserId, bool isAdmin, AiLogisticsQueryParameters query, CancellationToken cancellationToken);

    Task<DeliveryPredictionDto> GetDeliveryPredictionByIdAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);

    Task DeleteDeliveryPredictionAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);

    // ---- Route optimization -------------------------------------------
    Task<RouteOptimizationRunDto> OptimizeRouteAsync(
        Guid currentUserId, bool isAdmin, OptimizeRouteAiRequest request, CancellationToken cancellationToken);

    Task<RouteOptimizationRunDto> ApplyRouteOptimizationAsync(
        Guid currentUserId, bool isAdmin, Guid runId, CancellationToken cancellationToken);

    Task<PagedResult<RouteOptimizationRunListItemDto>> GetRouteOptimizationRunsAsync(
        Guid currentUserId, bool isAdmin, AiLogisticsQueryParameters query, CancellationToken cancellationToken);

    Task<RouteOptimizationRunDto> GetRouteOptimizationRunByIdAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);

    Task DeleteRouteOptimizationRunAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);

    // ---- Demand forecast --------------------------------------------
    Task<DemandForecastDto> ForecastDemandAsync(
        Guid currentUserId, bool isAdmin, ForecastDemandRequest request, CancellationToken cancellationToken);

    Task<PagedResult<DemandForecastListItemDto>> GetDemandForecastsAsync(
        Guid currentUserId, bool isAdmin, AiLogisticsQueryParameters query, CancellationToken cancellationToken);

    Task<DemandForecastDto> GetDemandForecastByIdAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);

    Task DeleteDemandForecastAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);

    // ---- Smart warehouse allocation ------------------------------
    Task<WarehouseAllocationRecommendationDto> RecommendWarehouseAsync(
        Guid currentUserId, bool isAdmin, RecommendWarehouseRequest request, CancellationToken cancellationToken);

    Task<PagedResult<WarehouseAllocationRecommendationListItemDto>> GetWarehouseAllocationsAsync(
        Guid currentUserId, bool isAdmin, AiLogisticsQueryParameters query, CancellationToken cancellationToken);

    Task<WarehouseAllocationRecommendationDto> GetWarehouseAllocationByIdAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);

    Task DeleteWarehouseAllocationAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);
}
