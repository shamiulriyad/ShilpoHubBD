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
[Route("api/logistics/returns")]
[Authorize(Roles = $"{RoleNames.LogisticsPartner},{RoleNames.SuperAdmin}")]
public class ReturnsController : ControllerBase
{
    private readonly IReturnHandlingService _service;

    public ReturnsController(IReturnHandlingService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet]
    public async Task<ActionResult<PagedResult<ReturnRequestListItemDto>>> GetPaged(
        [FromQuery] ReturnRequestQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetPagedAsync(CurrentUserId, IsAdmin, query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReturnRequestDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetByIdAsync(CurrentUserId, IsAdmin, id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ReturnRequestDto>> Create(
        CreateReturnRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(CurrentUserId, IsAdmin, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ReturnRequestDto>> Update(
        Guid id, UpdateReturnRequestRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ReturnRequestDto>> Approve(
        Guid id, ApproveReturnRequestRequest request, CancellationToken cancellationToken)
        => Ok(await _service.ApproveAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ReturnRequestDto>> Reject(
        Guid id, RejectReturnRequestRequest request, CancellationToken cancellationToken)
        => Ok(await _service.RejectAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/schedule-pickup")]
    public async Task<ActionResult<ReturnRequestDto>> SchedulePickup(
        Guid id, ScheduleReturnPickupRequest request, CancellationToken cancellationToken)
        => Ok(await _service.SchedulePickupAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/status")]
    public async Task<ActionResult<ReturnRequestDto>> UpdateStatus(
        Guid id, UpdateReturnStatusRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateStatusAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/inspections")]
    public async Task<ActionResult<ReturnRequestDto>> RecordInspection(
        Guid id, RecordReturnInspectionRequest request, CancellationToken cancellationToken)
        => Ok(await _service.RecordInspectionAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/restock")]
    public async Task<ActionResult<ReturnRequestDto>> Restock(
        Guid id, RestockReturnRequest request, CancellationToken cancellationToken)
        => Ok(await _service.RestockAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/refund")]
    public async Task<ActionResult<ReturnRequestDto>> RecordRefund(
        Guid id, RecordReturnRefundRequest request, CancellationToken cancellationToken)
        => Ok(await _service.RecordRefundAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult<ReturnRequestDto>> Close(
        Guid id, CloseReturnRequestRequest request, CancellationToken cancellationToken)
        => Ok(await _service.CloseAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ReturnRequestDto>> Cancel(
        Guid id, CancelReturnRequestRequest request, CancellationToken cancellationToken)
        => Ok(await _service.CancelAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/notes")]
    public async Task<ActionResult<ReturnRequestDto>> AddNote(
        Guid id, AddReturnNoteRequest request, CancellationToken cancellationToken)
        => Ok(await _service.AddNoteAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(CurrentUserId, IsAdmin, id, cancellationToken);
        return NoContent();
    }
}
