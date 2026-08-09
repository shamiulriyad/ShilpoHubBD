using ShilpoHubBD.Application.DTOs.BusinessPartnerAnalytics;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.BusinessPartnerAnalytics;

public class BusinessPartnerAnalyticsService : IBusinessPartnerAnalyticsService
{
    private readonly IBusinessPartnerAnalyticsRepository _repository;
    private readonly ICategoryRepository _categoryRepository;

    public BusinessPartnerAnalyticsService(IBusinessPartnerAnalyticsRepository repository, ICategoryRepository categoryRepository)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
    }

    public Task<List<CategoryDemandDto>> GetMarketDemandAsync(AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
        => _repository.GetMarketDemandAsync(parameters, cancellationToken);

    public Task<List<MonthlyTrendDto>> GetExportTrendsAsync(AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
        => _repository.GetExportTrendsAsync(parameters, cancellationToken);

    public async Task<ProductionForecastDto> GetProductionForecastAsync(Guid categoryId, int horizonMonths, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        var history = await _repository.GetCategoryMonthlyQuantityAsync(categoryId, 12, cancellationToken);

        if (history.Count == 0)
        {
            return new ProductionForecastDto
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                HistoricalMonthlyQuantity = history,
                ForecastedMonthlyQuantity = new List<MonthlyTrendDto>(),
                Trend = "Insufficient data",
            };
        }

        var average = history.Average(h => h.Quantity);
        var slope = LinearSlope(history.Select(h => (double)h.Quantity).ToList());

        var trend = slope switch
        {
            > 0.3 => "Increasing",
            < -0.3 => "Decreasing",
            _ => "Stable",
        };

        var forecast = new List<MonthlyTrendDto>();
        var lastPeriod = history[^1].PeriodStart;
        for (var i = 1; i <= horizonMonths; i++)
        {
            var projected = average + (slope * (history.Count + i - 1));
            forecast.Add(new MonthlyTrendDto
            {
                PeriodStart = new DateTime(lastPeriod.Year, lastPeriod.Month, 1).AddMonths(i),
                Quantity = Math.Max(0, (int)Math.Round(projected)),
            });
        }

        return new ProductionForecastDto
        {
            CategoryId = category.Id,
            CategoryName = category.Name,
            HistoricalMonthlyQuantity = history,
            ForecastedMonthlyQuantity = forecast,
            Trend = trend,
        };
    }

    public Task<List<IndustryInsightDto>> GetIndustryInsightsAsync(AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
        => _repository.GetIndustryInsightsAsync(parameters, cancellationToken);

    public Task<ProcurementAnalyticsDto> GetProcurementAnalyticsAsync(
        Guid businessPartnerId, bool isAdmin, AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
        => _repository.GetProcurementAnalyticsAsync(isAdmin ? null : businessPartnerId, parameters, cancellationToken);

    public Task<List<SupplierPerformanceDto>> GetSupplierPerformanceAsync(
        Guid businessPartnerId, bool isAdmin, AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
        => _repository.GetSupplierPerformanceAsync(isAdmin ? null : businessPartnerId, parameters, cancellationToken);

    public Task<SpendingAnalyticsDto> GetSpendingAnalyticsAsync(
        Guid businessPartnerId, bool isAdmin, AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
        => _repository.GetSpendingAnalyticsAsync(isAdmin ? null : businessPartnerId, parameters, cancellationToken);

    public Task<List<MonthlyTrendDto>> GetOrderTrendsAsync(
        Guid businessPartnerId, bool isAdmin, AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
        => _repository.GetOrderTrendsAsync(isAdmin ? null : businessPartnerId, parameters, cancellationToken);

    private static double LinearSlope(List<double> values)
    {
        var n = values.Count;
        if (n < 2)
        {
            return 0;
        }

        var xMean = (n - 1) / 2.0;
        var yMean = values.Average();

        double numerator = 0;
        double denominator = 0;
        for (var i = 0; i < n; i++)
        {
            numerator += (i - xMean) * (values[i] - yMean);
            denominator += (i - xMean) * (i - xMean);
        }

        return denominator == 0 ? 0 : numerator / denominator;
    }
}
