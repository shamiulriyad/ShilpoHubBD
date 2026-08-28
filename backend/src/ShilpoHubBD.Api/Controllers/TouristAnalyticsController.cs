using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.TouristAnalytics;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/tourist-analytics")]
[Authorize]
public class TouristAnalyticsController : ControllerBase
{
    private readonly ITouristAnalyticsService _analyticsService;

    public TouristAnalyticsController(ITouristAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("visited-locations")]
    public async Task<ActionResult<List<VisitedLocationDto>>> GetVisitedLocations(CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetVisitedLocationsAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("popular-destinations")]
    public async Task<ActionResult<List<PopularDestinationDto>>> GetPopularDestinations(
        CancellationToken cancellationToken, [FromQuery] int count = 10)
    {
        var result = await _analyticsService.GetPopularDestinationsAsync(count, cancellationToken);
        return Ok(result);
    }

    [HttpGet("bookings")]
    public async Task<ActionResult<BookingStatisticsDto>> GetBookingStatistics(CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetBookingStatisticsAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("spending")]
    public async Task<ActionResult<List<TravelSpendingByMonthDto>>> GetSpendingByMonth(
        CancellationToken cancellationToken, [FromQuery] int months = 12)
    {
        var result = await _analyticsService.GetTravelSpendingByMonthAsync(CurrentUserId, months, cancellationToken);
        return Ok(result);
    }

    [HttpGet("favorite-categories")]
    public async Task<ActionResult<List<FavoriteBookingCategoryDto>>> GetFavoriteCategories(
        CancellationToken cancellationToken, [FromQuery] int count = 5)
    {
        var result = await _analyticsService.GetFavoriteBookingCategoriesAsync(CurrentUserId, count, cancellationToken);
        return Ok(result);
    }

    [HttpGet("festival-participation")]
    public async Task<ActionResult<FestivalParticipationDto>> GetFestivalParticipation(CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetFestivalParticipationAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("district-coverage")]
    public async Task<ActionResult<DistrictCoverageDto>> GetDistrictCoverage(CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetDistrictCoverageAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("achievements")]
    public async Task<ActionResult<CulturalAchievementsSummaryDto>> GetCulturalAchievements(CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetCulturalAchievementsSummaryAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
