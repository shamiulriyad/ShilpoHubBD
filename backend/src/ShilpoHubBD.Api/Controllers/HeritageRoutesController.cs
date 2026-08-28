using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/heritage-routes")]
public class HeritageRoutesController : ControllerBase
{
    private readonly IHeritageRouteService _routeService;

    public HeritageRoutesController(IHeritageRouteService routeService)
    {
        _routeService = routeService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<HeritageRouteDto>>> GetPaged(
        [FromQuery] HeritageRouteQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _routeService.GetPagedAsync(query, cancellationToken));

    [HttpGet("recommended")]
    public async Task<ActionResult<List<HeritageRouteDto>>> GetRecommended(CancellationToken cancellationToken)
        => Ok(await _routeService.GetRecommendedAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<HeritageRouteDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _routeService.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<HeritageRouteDto>> Create(CreateHeritageRouteRequest request, CancellationToken cancellationToken)
    {
        var result = await _routeService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<HeritageRouteDto>> Update(Guid id, UpdateHeritageRouteRequest request, CancellationToken cancellationToken)
        => Ok(await _routeService.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _routeService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{id:guid}/stops")]
    public async Task<ActionResult<HeritageRouteDto>> AddStop(Guid id, CreateRouteStopRequest request, CancellationToken cancellationToken)
        => Ok(await _routeService.AddStopAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}/stops/{stopId:guid}")]
    public async Task<ActionResult<HeritageRouteDto>> RemoveStop(Guid id, Guid stopId, CancellationToken cancellationToken)
        => Ok(await _routeService.RemoveStopAsync(id, stopId, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}/stops/reorder")]
    public async Task<ActionResult<HeritageRouteDto>> ReorderStops(Guid id, ReorderStopsRequest request, CancellationToken cancellationToken)
        => Ok(await _routeService.ReorderStopsAsync(id, request, cancellationToken));
}
