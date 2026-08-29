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
[Route("api/logistics/partners")]
[Authorize(Roles = $"{RoleNames.LogisticsPartner},{RoleNames.SuperAdmin}")]
public class LogisticsPartnersController : ControllerBase
{
    private readonly ILogisticsPartnerService _service;

    public LogisticsPartnersController(ILogisticsPartnerService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<PagedResult<LogisticsPartnerProfileListItemDto>>> GetPaged(
        [FromQuery] LogisticsPartnerQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetPagedAsync(query, cancellationToken));

    [HttpGet("me")]
    public async Task<ActionResult<LogisticsPartnerProfileDto>> GetMine(CancellationToken cancellationToken)
        => Ok(await _service.GetByUserIdAsync(CurrentUserId, cancellationToken));

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<LogisticsPartnerProfileDto>> GetByUserId(
        Guid userId, CancellationToken cancellationToken)
    {
        if (!IsAdmin && userId != CurrentUserId)
        {
            return Forbid();
        }

        return Ok(await _service.GetByUserIdAsync(userId, cancellationToken));
    }

    [HttpPut("{userId:guid}")]
    public async Task<ActionResult<LogisticsPartnerProfileDto>> Upsert(
        Guid userId, UpsertLogisticsPartnerProfileRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpsertAsync(userId, CurrentUserId, IsAdmin, request, cancellationToken));

    [HttpPost("{userId:guid}/verify")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<LogisticsPartnerProfileDto>> Verify(
        Guid userId, VerifyLogisticsPartnerRequest request, CancellationToken cancellationToken)
        => Ok(await _service.VerifyAsync(userId, CurrentUserId, request, cancellationToken));

    [HttpPut("{userId:guid}/service-areas")]
    public async Task<ActionResult<LogisticsPartnerProfileDto>> UpsertServiceArea(
        Guid userId, UpsertLogisticsServiceAreaRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpsertServiceAreaAsync(userId, CurrentUserId, IsAdmin, request, cancellationToken));

    [HttpDelete("{userId:guid}/service-areas/{serviceAreaId:guid}")]
    public async Task<ActionResult<LogisticsPartnerProfileDto>> RemoveServiceArea(
        Guid userId, Guid serviceAreaId, CancellationToken cancellationToken)
        => Ok(await _service.RemoveServiceAreaAsync(userId, CurrentUserId, IsAdmin, serviceAreaId, cancellationToken));

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Delete(Guid userId, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(userId, CurrentUserId, IsAdmin, cancellationToken);
        return NoContent();
    }
}
