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

    [HttpGet("available")]
    [AllowAnonymous]
    public async Task<ActionResult<List<AvailableLogisticsOptionDto>>> GetAvailable(
        [FromQuery] Guid districtId, [FromQuery] string? areaName, CancellationToken cancellationToken)
        => Ok(await _service.GetAvailableAsync(districtId, areaName, cancellationToken));

    [HttpPost("official")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<LogisticsPartnerProfileDto>> CreateOfficial(
        UpsertLogisticsPartnerProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateOfficialAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetOfficial), new { profileId = result.Id }, result);
    }

    [HttpGet("official/{profileId:guid}")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<LogisticsPartnerProfileDto>> GetOfficial(Guid profileId, CancellationToken cancellationToken)
        => Ok(await _service.GetByIdAsync(profileId, cancellationToken));

    [HttpGet("official/{profileId:guid}/performance")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<LogisticsPartnerPerformanceDto>> GetPerformance(Guid profileId, CancellationToken cancellationToken)
        => Ok(await _service.GetPerformanceAsync(profileId, cancellationToken));

    [HttpPut("official/{profileId:guid}")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<LogisticsPartnerProfileDto>> UpdateOfficial(
        Guid profileId, UpsertLogisticsPartnerProfileRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateOfficialAsync(profileId, CurrentUserId, request, cancellationToken));

    [HttpPut("official/{profileId:guid}/service-areas")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<LogisticsPartnerProfileDto>> UpsertOfficialServiceArea(
        Guid profileId, UpsertLogisticsServiceAreaRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpsertOfficialServiceAreaAsync(profileId, request, cancellationToken));

    [HttpDelete("official/{profileId:guid}/service-areas/{serviceAreaId:guid}")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<LogisticsPartnerProfileDto>> RemoveOfficialServiceArea(
        Guid profileId, Guid serviceAreaId, CancellationToken cancellationToken)
        => Ok(await _service.RemoveOfficialServiceAreaAsync(profileId, serviceAreaId, cancellationToken));

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
