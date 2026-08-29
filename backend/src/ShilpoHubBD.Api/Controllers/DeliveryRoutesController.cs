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
[Route("api/logistics/routes")]
[Authorize(Roles = $"{RoleNames.LogisticsPartner},{RoleNames.SuperAdmin}")]
public class DeliveryRoutesController : ControllerBase
{
    private readonly IRouteOptimizationService _service;

    public DeliveryRoutesController(IRouteOptimizationService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet]
    public async Task<ActionResult<PagedResult<DeliveryRouteListItemDto>>> GetPaged(
        [FromQuery] DeliveryRouteQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetPagedAsync(CurrentUserId, IsAdmin, query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DeliveryRouteDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetByIdAsync(CurrentUserId, IsAdmin, id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<DeliveryRouteDto>> Create(
        CreateDeliveryRouteRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(CurrentUserId, IsAdmin, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DeliveryRouteDto>> Update(
        Guid id, UpdateDeliveryRouteRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/stops")]
    public async Task<ActionResult<DeliveryRouteDto>> AddStop(
        Guid id, RouteStopInput request, CancellationToken cancellationToken)
        => Ok(await _service.AddStopAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPut("{id:guid}/stops/{stopId:guid}")]
    public async Task<ActionResult<DeliveryRouteDto>> UpdateStop(
        Guid id, Guid stopId, UpdateRouteStopRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateStopAsync(CurrentUserId, IsAdmin, id, stopId, request, cancellationToken));

    [HttpDelete("{id:guid}/stops/{stopId:guid}")]
    public async Task<ActionResult<DeliveryRouteDto>> RemoveStop(
        Guid id, Guid stopId, CancellationToken cancellationToken)
        => Ok(await _service.RemoveStopAsync(CurrentUserId, IsAdmin, id, stopId, cancellationToken));

    [HttpPost("{id:guid}/resequence")]
    public async Task<ActionResult<DeliveryRouteDto>> Resequence(
        Guid id, ResequenceRouteRequest request, CancellationToken cancellationToken)
        => Ok(await _service.ResequenceAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/optimize")]
    public async Task<ActionResult<DeliveryRouteDto>> Optimize(
        Guid id, OptimizeRouteRequest request, CancellationToken cancellationToken)
        => Ok(await _service.OptimizeAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/assign")]
    public async Task<ActionResult<DeliveryRouteDto>> Assign(
        Guid id, AssignRouteRequest request, CancellationToken cancellationToken)
        => Ok(await _service.AssignAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/dispatch")]
    public async Task<ActionResult<DeliveryRouteDto>> Dispatch(
        Guid id, RouteTransitionRequest request, CancellationToken cancellationToken)
        => Ok(await _service.DispatchAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<DeliveryRouteDto>> Start(
        Guid id, RouteTransitionRequest request, CancellationToken cancellationToken)
        => Ok(await _service.StartAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<DeliveryRouteDto>> Complete(
        Guid id, RouteTransitionRequest request, CancellationToken cancellationToken)
        => Ok(await _service.CompleteAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<DeliveryRouteDto>> Cancel(
        Guid id, CancelRouteRequest request, CancellationToken cancellationToken)
        => Ok(await _service.CancelAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/stops/{stopId:guid}/arrive")]
    public async Task<ActionResult<DeliveryRouteDto>> ArriveStop(
        Guid id, Guid stopId, CancellationToken cancellationToken)
        => Ok(await _service.ArriveStopAsync(CurrentUserId, IsAdmin, id, stopId, cancellationToken));

    [HttpPost("{id:guid}/stops/{stopId:guid}/complete")]
    public async Task<ActionResult<DeliveryRouteDto>> CompleteStop(
        Guid id, Guid stopId, CompleteRouteStopRequest request, CancellationToken cancellationToken)
        => Ok(await _service.CompleteStopAsync(CurrentUserId, IsAdmin, id, stopId, request, cancellationToken));

    [HttpPost("{id:guid}/stops/{stopId:guid}/skip")]
    public async Task<ActionResult<DeliveryRouteDto>> SkipStop(
        Guid id, Guid stopId, FailRouteStopRequest request, CancellationToken cancellationToken)
        => Ok(await _service.SkipStopAsync(CurrentUserId, IsAdmin, id, stopId, request, cancellationToken));

    [HttpPost("{id:guid}/stops/{stopId:guid}/fail")]
    public async Task<ActionResult<DeliveryRouteDto>> FailStop(
        Guid id, Guid stopId, FailRouteStopRequest request, CancellationToken cancellationToken)
        => Ok(await _service.FailStopAsync(CurrentUserId, IsAdmin, id, stopId, request, cancellationToken));

    [HttpPost("{id:guid}/notes")]
    public async Task<ActionResult<DeliveryRouteDto>> AddNote(
        Guid id, AddRouteNoteRequest request, CancellationToken cancellationToken)
        => Ok(await _service.AddNoteAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(CurrentUserId, IsAdmin, id, cancellationToken);
        return NoContent();
    }
}
