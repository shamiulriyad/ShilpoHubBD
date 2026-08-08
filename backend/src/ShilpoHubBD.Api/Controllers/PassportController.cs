using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Passport;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/passport")]
public class PassportController : ControllerBase
{
    private readonly IPassportService _passportService;

    public PassportController(IPassportService passportService)
    {
        _passportService = passportService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("badges")]
    public async Task<ActionResult<List<BadgeDto>>> GetAllBadges(CancellationToken cancellationToken)
    {
        var result = await _passportService.GetAllBadgesAsync(cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("badges/mine")]
    public async Task<ActionResult<List<UserBadgeDto>>> GetMyBadges(CancellationToken cancellationToken)
    {
        var result = await _passportService.GetMyBadgesAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("badges")]
    public async Task<ActionResult<BadgeDto>> CreateBadge(CreateBadgeRequest request, CancellationToken cancellationToken)
    {
        var result = await _passportService.CreateBadgeAsync(request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("badges/claim/district")]
    public async Task<ActionResult<UserBadgeDto>> ClaimDistrictBadge(ClaimDistrictBadgeRequest request, CancellationToken cancellationToken)
    {
        var result = await _passportService.ClaimDistrictBadgeAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("badges/claim/festival")]
    public async Task<ActionResult<UserBadgeDto>> ClaimFestivalBadge(ClaimFestivalBadgeRequest request, CancellationToken cancellationToken)
    {
        var result = await _passportService.ClaimFestivalBadgeAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("badges/evaluate-purchases")]
    public async Task<ActionResult<List<UserBadgeDto>>> EvaluatePurchaseBadges(CancellationToken cancellationToken)
    {
        var result = await _passportService.EvaluatePurchaseBadgesAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
