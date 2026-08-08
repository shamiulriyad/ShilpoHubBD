using ShilpoHubBD.Application.DTOs.Analytics;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _analyticsRepository;

    public AnalyticsService(IAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    public Task<PurchaseAnalyticsDto> GetPurchaseAnalyticsAsync(Guid userId, CancellationToken cancellationToken)
        => _analyticsRepository.GetPurchaseAnalyticsAsync(userId, cancellationToken);

    public Task<List<SpendingByMonthDto>> GetSpendingByMonthAsync(Guid userId, int months, CancellationToken cancellationToken)
        => _analyticsRepository.GetSpendingByMonthAsync(userId, Math.Clamp(months, 1, 36), cancellationToken);

    public Task<List<FavoriteCategoryDto>> GetFavoriteCategoriesAsync(Guid userId, int count, CancellationToken cancellationToken)
        => _analyticsRepository.GetFavoriteCategoriesAsync(userId, Math.Clamp(count, 1, 20), cancellationToken);
}
