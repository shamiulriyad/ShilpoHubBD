using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Analytics;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("purchases")]
    public async Task<ActionResult<PurchaseAnalyticsDto>> GetPurchaseAnalytics(CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetPurchaseAnalyticsAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("spending")]
    public async Task<ActionResult<List<SpendingByMonthDto>>> GetSpendingByMonth(
        CancellationToken cancellationToken, [FromQuery] int months = 12)
    {
        var result = await _analyticsService.GetSpendingByMonthAsync(CurrentUserId, months, cancellationToken);
        return Ok(result);
    }

    [HttpGet("favorite-categories")]
    public async Task<ActionResult<List<FavoriteCategoryDto>>> GetFavoriteCategories(
        CancellationToken cancellationToken, [FromQuery] int count = 5)
    {
        var result = await _analyticsService.GetFavoriteCategoriesAsync(CurrentUserId, count, cancellationToken);
        return Ok(result);
    }
}
