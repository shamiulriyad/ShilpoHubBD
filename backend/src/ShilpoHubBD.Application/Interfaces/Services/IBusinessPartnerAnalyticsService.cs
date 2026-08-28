using ShilpoHubBD.Application.DTOs.BusinessPartnerAnalytics;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IBusinessPartnerAnalyticsService
{
    Task<List<CategoryDemandDto>> GetMarketDemandAsync(AnalyticsQueryParameters parameters, CancellationToken cancellationToken);
    Task<List<MonthlyTrendDto>> GetExportTrendsAsync(AnalyticsQueryParameters parameters, CancellationToken cancellationToken);
    Task<ProductionForecastDto> GetProductionForecastAsync(Guid categoryId, int horizonMonths, CancellationToken cancellationToken);
    Task<List<IndustryInsightDto>> GetIndustryInsightsAsync(AnalyticsQueryParameters parameters, CancellationToken cancellationToken);
    Task<ProcurementAnalyticsDto> GetProcurementAnalyticsAsync(Guid businessPartnerId, bool isAdmin, AnalyticsQueryParameters parameters, CancellationToken cancellationToken);
    Task<List<SupplierPerformanceDto>> GetSupplierPerformanceAsync(Guid businessPartnerId, bool isAdmin, AnalyticsQueryParameters parameters, CancellationToken cancellationToken);
    Task<SpendingAnalyticsDto> GetSpendingAnalyticsAsync(Guid businessPartnerId, bool isAdmin, AnalyticsQueryParameters parameters, CancellationToken cancellationToken);
    Task<List<MonthlyTrendDto>> GetOrderTrendsAsync(Guid businessPartnerId, bool isAdmin, AnalyticsQueryParameters parameters, CancellationToken cancellationToken);
}
