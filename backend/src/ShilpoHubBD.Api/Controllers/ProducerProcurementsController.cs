using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Procurement;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

// Bulk procurement requests business partners addressed to this producer.
[ApiController]
[Route("api/producer/procurements")]
[Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
public class ProducerProcurementsController : ControllerBase
{
    private readonly IProcurementService _service;

    public ProducerProcurementsController(IProcurementService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProcurementRequestListItemDto>>> GetMine(
        [FromQuery] ProcurementQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetForProducerAsync(CurrentUserId, query, cancellationToken));

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ProcurementRequestDto>> Approve(Guid id, ProcurementDecisionRequest request, CancellationToken cancellationToken)
        => Ok(await _service.ApproveAsync(id, CurrentUserId, IsAdmin, request, cancellationToken));

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ProcurementRequestDto>> Reject(Guid id, ProcurementDecisionRequest request, CancellationToken cancellationToken)
        => Ok(await _service.RejectAsync(id, CurrentUserId, IsAdmin, request, cancellationToken));
}
