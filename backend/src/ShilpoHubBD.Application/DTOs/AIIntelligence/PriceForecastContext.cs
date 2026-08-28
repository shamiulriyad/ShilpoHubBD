namespace ShilpoHubBD.Application.DTOs.AIIntelligence;

public class PriceForecastContext
{
    public string CategoryName { get; set; } = string.Empty;
    public int HorizonMonths { get; set; }
    public List<PeriodPriceDto> HistoricalMonthlyAveragePrice { get; set; } = new();
}
