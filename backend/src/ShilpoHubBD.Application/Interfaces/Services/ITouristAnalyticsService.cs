using ShilpoHubBD.Application.DTOs.TouristAnalytics;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ITouristAnalyticsService
{
    Task<List<VisitedLocationDto>> GetVisitedLocationsAsync(Guid userId, CancellationToken cancellationToken);
    Task<List<PopularDestinationDto>> GetPopularDestinationsAsync(int count, CancellationToken cancellationToken);
    Task<BookingStatisticsDto> GetBookingStatisticsAsync(Guid userId, CancellationToken cancellationToken);
    Task<List<TravelSpendingByMonthDto>> GetTravelSpendingByMonthAsync(Guid userId, int months, CancellationToken cancellationToken);
    Task<List<FavoriteBookingCategoryDto>> GetFavoriteBookingCategoriesAsync(Guid userId, int count, CancellationToken cancellationToken);
    Task<FestivalParticipationDto> GetFestivalParticipationAsync(Guid userId, CancellationToken cancellationToken);
    Task<DistrictCoverageDto> GetDistrictCoverageAsync(Guid userId, CancellationToken cancellationToken);
    Task<CulturalAchievementsSummaryDto> GetCulturalAchievementsSummaryAsync(Guid userId, CancellationToken cancellationToken);
}
