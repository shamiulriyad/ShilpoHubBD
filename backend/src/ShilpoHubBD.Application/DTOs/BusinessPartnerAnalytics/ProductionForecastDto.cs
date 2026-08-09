namespace ShilpoHubBD.Application.DTOs.BusinessPartnerAnalytics;

public class ProductionForecastDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<MonthlyTrendDto> HistoricalMonthlyQuantity { get; set; } = new();
    public List<MonthlyTrendDto> ForecastedMonthlyQuantity { get; set; } = new();
    public string Trend { get; set; } = string.Empty;
}
