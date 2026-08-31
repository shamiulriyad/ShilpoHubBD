using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/logistics/ai")]
[Authorize(Roles = $"{RoleNames.LogisticsPartner},{RoleNames.SuperAdmin}")]
public class AiLogisticsController : ControllerBase
{
    private readonly IAiLogisticsService _service;

    public AiLogisticsController(IAiLogisticsService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    // ---- Delivery prediction -------------------------------------------

    [HttpPost("delivery-predictions")]
    public async Task<ActionResult<DeliveryPredictionDto>> PredictDelivery(
        PredictDeliveryRequest request, CancellationToken cancellationToken)
        => Ok(await _service.PredictDeliveryAsync(CurrentUserId, IsAdmin, request, cancellationToken));

    [HttpGet("delivery-predictions")]
    public async Task<ActionResult<PagedResult<DeliveryPredictionListItemDto>>> GetDeliveryPredictions(
        [FromQuery] AiLogisticsQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetDeliveryPredictionsAsync(CurrentUserId, IsAdmin, query, cancellationToken));

    [HttpGet("delivery-predictions/{id:guid}")]
    public async Task<ActionResult<DeliveryPredictionDto>> GetDeliveryPrediction(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetDeliveryPredictionByIdAsync(CurrentUserId, IsAdmin, id, cancellationToken));

    [HttpDelete("delivery-predictions/{id:guid}")]
    public async Task<IActionResult> DeleteDeliveryPrediction(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteDeliveryPredictionAsync(CurrentUserId, IsAdmin, id, cancellationToken);
        return NoContent();
    }

    // ---- Route optimization -------------------------------------------

    [HttpPost("route-optimizations")]
    public async Task<ActionResult<RouteOptimizationRunDto>> OptimizeRoute(
        OptimizeRouteAiRequest request, CancellationToken cancellationToken)
        => Ok(await _service.OptimizeRouteAsync(CurrentUserId, IsAdmin, request, cancellationToken));

    [HttpPost("route-optimizations/{id:guid}/apply")]
    public async Task<ActionResult<RouteOptimizationRunDto>> ApplyRouteOptimization(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.ApplyRouteOptimizationAsync(CurrentUserId, IsAdmin, id, cancellationToken));

    [HttpGet("route-optimizations")]
    public async Task<ActionResult<PagedResult<RouteOptimizationRunListItemDto>>> GetRouteOptimizations(
        [FromQuery] AiLogisticsQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetRouteOptimizationRunsAsync(CurrentUserId, IsAdmin, query, cancellationToken));

    [HttpGet("route-optimizations/{id:guid}")]
    public async Task<ActionResult<RouteOptimizationRunDto>> GetRouteOptimization(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetRouteOptimizationRunByIdAsync(CurrentUserId, IsAdmin, id, cancellationToken));

    [HttpDelete("route-optimizations/{id:guid}")]
    public async Task<IActionResult> DeleteRouteOptimization(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteRouteOptimizationRunAsync(CurrentUserId, IsAdmin, id, cancellationToken);
        return NoContent();
    }

    // ---- Demand forecast --------------------------------------------

    [HttpPost("demand-forecasts")]
    public async Task<ActionResult<DemandForecastDto>> ForecastDemand(
        ForecastDemandRequest request, CancellationToken cancellationToken)
        => Ok(await _service.ForecastDemandAsync(CurrentUserId, IsAdmin, request, cancellationToken));

    [HttpGet("demand-forecasts")]
    public async Task<ActionResult<PagedResult<DemandForecastListItemDto>>> GetDemandForecasts(
        [FromQuery] AiLogisticsQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetDemandForecastsAsync(CurrentUserId, IsAdmin, query, cancellationToken));

    [HttpGet("demand-forecasts/{id:guid}")]
    public async Task<ActionResult<DemandForecastDto>> GetDemandForecast(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetDemandForecastByIdAsync(CurrentUserId, IsAdmin, id, cancellationToken));

    [HttpDelete("demand-forecasts/{id:guid}")]
    public async Task<IActionResult> DeleteDemandForecast(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteDemandForecastAsync(CurrentUserId, IsAdmin, id, cancellationToken);
        return NoContent();
    }

    // ---- Smart warehouse allocation ------------------------------

    [HttpPost("warehouse-allocations")]
    public async Task<ActionResult<WarehouseAllocationRecommendationDto>> RecommendWarehouse(
        RecommendWarehouseRequest request, CancellationToken cancellationToken)
        => Ok(await _service.RecommendWarehouseAsync(CurrentUserId, IsAdmin, request, cancellationToken));

    [HttpGet("warehouse-allocations")]
    public async Task<ActionResult<PagedResult<WarehouseAllocationRecommendationListItemDto>>> GetWarehouseAllocations(
        [FromQuery] AiLogisticsQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetWarehouseAllocationsAsync(CurrentUserId, IsAdmin, query, cancellationToken));

    [HttpGet("warehouse-allocations/{id:guid}")]
    public async Task<ActionResult<WarehouseAllocationRecommendationDto>> GetWarehouseAllocation(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetWarehouseAllocationByIdAsync(CurrentUserId, IsAdmin, id, cancellationToken));

    [HttpDelete("warehouse-allocations/{id:guid}")]
    public async Task<IActionResult> DeleteWarehouseAllocation(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteWarehouseAllocationAsync(CurrentUserId, IsAdmin, id, cancellationToken);
        return NoContent();
    }
}
