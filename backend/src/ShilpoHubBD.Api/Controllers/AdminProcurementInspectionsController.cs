using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Procurement;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

// Admin inspection of bulk deals whose advance has been paid.
[ApiController]
[Route("api/admin/procurements")]
[Authorize(Roles = RoleNames.SuperAdmin)]
public class AdminProcurementInspectionsController : ControllerBase
{
    private readonly IProcurementService _service;

    public AdminProcurementInspectionsController(IProcurementService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("inspections")]
    public async Task<ActionResult<PagedResult<ProcurementRequestListItemDto>>> GetInspections(
        [FromQuery] bool pendingOnly, [FromQuery] ProcurementQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetForInspectionAsync(pendingOnly, query, cancellationToken));

    [HttpPost("{id:guid}/inspect")]
    public async Task<ActionResult<ProcurementRequestDto>> Inspect(Guid id, ProcurementInspectionRequest request, CancellationToken cancellationToken)
        => Ok(await _service.InspectAsync(id, CurrentUserId, request, cancellationToken));
}
