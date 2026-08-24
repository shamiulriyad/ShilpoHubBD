using ShilpoHubBD.Application.DTOs.TouristAnalytics;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.Analytics;

public class TouristAnalyticsService : ITouristAnalyticsService
{
    private readonly ITouristAnalyticsRepository _analyticsRepository;
    private readonly IAchievementService _achievementService;

    public TouristAnalyticsService(ITouristAnalyticsRepository analyticsRepository, IAchievementService achievementService)
    {
        _analyticsRepository = analyticsRepository;
        _achievementService = achievementService;
    }

    public Task<List<VisitedLocationDto>> GetVisitedLocationsAsync(Guid userId, CancellationToken cancellationToken)
        => _analyticsRepository.GetVisitedLocationsAsync(userId, cancellationToken);

    public Task<List<PopularDestinationDto>> GetPopularDestinationsAsync(int count, CancellationToken cancellationToken)
        => _analyticsRepository.GetPopularDestinationsAsync(Math.Clamp(count, 1, 50), cancellationToken);

    public Task<BookingStatisticsDto> GetBookingStatisticsAsync(Guid userId, CancellationToken cancellationToken)
        => _analyticsRepository.GetBookingStatisticsAsync(userId, cancellationToken);

    public Task<List<TravelSpendingByMonthDto>> GetTravelSpendingByMonthAsync(Guid userId, int months, CancellationToken cancellationToken)
        => _analyticsRepository.GetTravelSpendingByMonthAsync(userId, Math.Clamp(months, 1, 36), cancellationToken);

    public Task<List<FavoriteBookingCategoryDto>> GetFavoriteBookingCategoriesAsync(Guid userId, int count, CancellationToken cancellationToken)
        => _analyticsRepository.GetFavoriteBookingCategoriesAsync(userId, Math.Clamp(count, 1, 20), cancellationToken);

    public Task<FestivalParticipationDto> GetFestivalParticipationAsync(Guid userId, CancellationToken cancellationToken)
        => _analyticsRepository.GetFestivalParticipationAsync(userId, cancellationToken);

    public Task<DistrictCoverageDto> GetDistrictCoverageAsync(Guid userId, CancellationToken cancellationToken)
        => _analyticsRepository.GetDistrictCoverageAsync(userId, cancellationToken);

    public async Task<CulturalAchievementsSummaryDto> GetCulturalAchievementsSummaryAsync(Guid userId, CancellationToken cancellationToken)
    {
        var badgeCounts = await _analyticsRepository.GetBadgeCountsByTypeAsync(userId, cancellationToken);
        var xpSummary = await _achievementService.GetMyXpSummaryAsync(userId, cancellationToken);

        return new CulturalAchievementsSummaryDto
        {
            TotalBadges = badgeCounts.Values.Sum(),
            BadgeCountsByType = badgeCounts,
            XpSummary = xpSummary,
        };
    }
}
