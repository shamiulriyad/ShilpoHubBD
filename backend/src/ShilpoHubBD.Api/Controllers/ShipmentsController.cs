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
[Route("api/logistics/shipments")]
[Authorize(Roles = $"{RoleNames.LogisticsPartner},{RoleNames.SuperAdmin}")]
public class ShipmentsController : ControllerBase
{
    private readonly IDeliveryTrackingService _service;

    public ShipmentsController(IDeliveryTrackingService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    /// <summary>Public, PII-light tracking timeline for a tracking number.</summary>
    [HttpGet("track/{trackingNumber}")]
    [AllowAnonymous]
    public async Task<ActionResult<ShipmentTrackingDto>> Track(string trackingNumber, CancellationToken cancellationToken)
        => Ok(await _service.TrackByNumberAsync(trackingNumber, cancellationToken));

    [HttpGet]
    public async Task<ActionResult<PagedResult<ShipmentListItemDto>>> GetPaged(
        [FromQuery] ShipmentQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetPagedAsync(CurrentUserId, IsAdmin, query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShipmentDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetByIdAsync(CurrentUserId, IsAdmin, id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ShipmentDto>> Create(
        CreateShipmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(CurrentUserId, IsAdmin, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ShipmentDto>> Update(
        Guid id, UpdateShipmentRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/status")]
    public async Task<ActionResult<ShipmentDto>> UpdateStatus(
        Guid id, UpdateShipmentStatusRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateStatusAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/events")]
    public async Task<ActionResult<ShipmentDto>> AddEvent(
        Guid id, AddShipmentTrackingEventRequest request, CancellationToken cancellationToken)
        => Ok(await _service.AddTrackingEventAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/location")]
    public async Task<ActionResult<ShipmentDto>> UpdateLocation(
        Guid id, UpdateShipmentLocationRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateLocationAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/delivery-attempts")]
    public async Task<ActionResult<ShipmentDto>> RecordDeliveryAttempt(
        Guid id, RecordDeliveryAttemptRequest request, CancellationToken cancellationToken)
        => Ok(await _service.RecordDeliveryAttemptAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/deliver")]
    public async Task<ActionResult<ShipmentDto>> MarkDelivered(
        Guid id, MarkShipmentDeliveredRequest request, CancellationToken cancellationToken)
        => Ok(await _service.MarkDeliveredAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ShipmentDto>> Cancel(
        Guid id, CancelShipmentRequest request, CancellationToken cancellationToken)
        => Ok(await _service.CancelAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/notes")]
    public async Task<ActionResult<ShipmentDto>> AddNote(
        Guid id, AddShipmentNoteRequest request, CancellationToken cancellationToken)
        => Ok(await _service.AddNoteAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(CurrentUserId, IsAdmin, id, cancellationToken);
        return NoContent();
    }
}
