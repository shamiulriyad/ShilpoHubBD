using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.BusinessPartner;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/business-partners")]
[Authorize]
public class BusinessPartnersController : ControllerBase
{
    private readonly IBusinessPartnerService _businessPartnerService;

    public BusinessPartnersController(IBusinessPartnerService businessPartnerService)
    {
        _businessPartnerService = businessPartnerService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpGet]
    public async Task<ActionResult<PagedResult<BusinessPartnerProfileDto>>> GetPaged(
        [FromQuery] BusinessPartnerQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _businessPartnerService.GetPagedAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<BusinessPartnerProfileDto>> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        if (!IsAdmin && userId != CurrentUserId)
        {
            return Forbid();
        }

        var result = await _businessPartnerService.GetByUserIdAsync(userId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpPut("{userId:guid}")]
    public async Task<ActionResult<BusinessPartnerProfileDto>> Upsert(
        Guid userId, UpsertBusinessPartnerProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _businessPartnerService.UpsertAsync(userId, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{userId:guid}/verify")]
    public async Task<ActionResult<BusinessPartnerProfileDto>> Verify(
        Guid userId, VerifyBusinessPartnerRequest request, CancellationToken cancellationToken)
    {
        var result = await _businessPartnerService.VerifyAsync(userId, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Delete(Guid userId, CancellationToken cancellationToken)
    {
        await _businessPartnerService.DeleteAsync(userId, CurrentUserId, IsAdmin, cancellationToken);
        return NoContent();
    }
}
