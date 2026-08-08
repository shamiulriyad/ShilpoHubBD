using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Achievement;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/achievements")]
public class AchievementsController : ControllerBase
{
    private readonly IAchievementService _achievementService;

    public AchievementsController(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize]
    [HttpGet("xp/mine")]
    public async Task<ActionResult<XpSummaryDto>> GetMyXpSummary(CancellationToken cancellationToken)
    {
        var result = await _achievementService.GetMyXpSummaryAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("xp/mine/history")]
    public async Task<ActionResult<List<XpTransactionDto>>> GetMyXpHistory(CancellationToken cancellationToken)
    {
        var result = await _achievementService.GetMyXpHistoryAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<AchievementDto>>> GetAllAchievements(CancellationToken cancellationToken)
    {
        var result = await _achievementService.GetAllAchievementsAsync(cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<List<UserAchievementDto>>> GetMyAchievements(CancellationToken cancellationToken)
    {
        var result = await _achievementService.GetMyAchievementsAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<AchievementDto>> CreateAchievement(CreateAchievementRequest request, CancellationToken cancellationToken)
    {
        var result = await _achievementService.CreateAchievementAsync(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("xp/award")]
    public async Task<ActionResult<XpSummaryDto>> AwardXp(AwardXpRequest request, CancellationToken cancellationToken)
    {
        var result = await _achievementService.AwardXpAsync(request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("evaluate")]
    public async Task<ActionResult<List<UserAchievementDto>>> Evaluate(CancellationToken cancellationToken)
    {
        var result = await _achievementService.EvaluateAchievementsAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
