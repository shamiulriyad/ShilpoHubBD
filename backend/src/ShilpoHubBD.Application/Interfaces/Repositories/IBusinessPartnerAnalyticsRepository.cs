using ShilpoHubBD.Application.DTOs.BusinessPartnerAnalytics;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IBusinessPartnerAnalyticsRepository
{
    Task<List<CategoryDemandDto>> GetMarketDemandAsync(AnalyticsQueryParameters parameters, CancellationToken cancellationToken);
    Task<List<MonthlyTrendDto>> GetExportTrendsAsync(AnalyticsQueryParameters parameters, CancellationToken cancellationToken);
    Task<List<MonthlyTrendDto>> GetCategoryMonthlyQuantityAsync(Guid categoryId, int months, CancellationToken cancellationToken);
    Task<List<IndustryInsightDto>> GetIndustryInsightsAsync(AnalyticsQueryParameters parameters, CancellationToken cancellationToken);
    Task<ProcurementAnalyticsDto> GetProcurementAnalyticsAsync(Guid? businessPartnerId, AnalyticsQueryParameters parameters, CancellationToken cancellationToken);
    Task<List<SupplierPerformanceDto>> GetSupplierPerformanceAsync(Guid? businessPartnerId, AnalyticsQueryParameters parameters, CancellationToken cancellationToken);
    Task<SpendingAnalyticsDto> GetSpendingAnalyticsAsync(Guid? businessPartnerId, AnalyticsQueryParameters parameters, CancellationToken cancellationToken);
    Task<List<MonthlyTrendDto>> GetOrderTrendsAsync(Guid? businessPartnerId, AnalyticsQueryParameters parameters, CancellationToken cancellationToken);
}
