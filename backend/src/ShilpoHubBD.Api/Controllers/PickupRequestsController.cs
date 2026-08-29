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
[Route("api/logistics/pickups")]
[Authorize(Roles = $"{RoleNames.LogisticsPartner},{RoleNames.SuperAdmin}")]
public class PickupRequestsController : ControllerBase
{
    private readonly IPickupSchedulingService _service;

    public PickupRequestsController(IPickupSchedulingService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet]
    public async Task<ActionResult<PagedResult<PickupRequestListItemDto>>> GetPaged(
        [FromQuery] PickupRequestQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetPagedAsync(CurrentUserId, IsAdmin, query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PickupRequestDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetByIdAsync(CurrentUserId, IsAdmin, id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<PickupRequestDto>> Create(
        CreatePickupRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(CurrentUserId, IsAdmin, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PickupRequestDto>> Update(
        Guid id, UpdatePickupRequestRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/schedule")]
    public async Task<ActionResult<PickupRequestDto>> Schedule(
        Guid id, SchedulePickupRequestRequest request, CancellationToken cancellationToken)
        => Ok(await _service.ScheduleAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/assign")]
    public async Task<ActionResult<PickupRequestDto>> Assign(
        Guid id, AssignPickupRequestRequest request, CancellationToken cancellationToken)
        => Ok(await _service.AssignAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/status")]
    public async Task<ActionResult<PickupRequestDto>> UpdateStatus(
        Guid id, UpdatePickupStatusRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateStatusAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<PickupRequestDto>> Cancel(
        Guid id, CancelPickupRequestRequest request, CancellationToken cancellationToken)
        => Ok(await _service.CancelAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/notes")]
    public async Task<ActionResult<PickupRequestDto>> AddNote(
        Guid id, AddPickupNoteRequest request, CancellationToken cancellationToken)
        => Ok(await _service.AddNoteAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(CurrentUserId, IsAdmin, id, cancellationToken);
        return NoContent();
    }
}
