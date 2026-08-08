using ShilpoHubBD.Application.DTOs.Analytics;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IAnalyticsService
{
    Task<PurchaseAnalyticsDto> GetPurchaseAnalyticsAsync(Guid userId, CancellationToken cancellationToken);
    Task<List<SpendingByMonthDto>> GetSpendingByMonthAsync(Guid userId, int months, CancellationToken cancellationToken);
    Task<List<FavoriteCategoryDto>> GetFavoriteCategoriesAsync(Guid userId, int count, CancellationToken cancellationToken);
}
