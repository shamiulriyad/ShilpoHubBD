namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class DemandForecastContext
{
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int HorizonWeeks { get; set; }
    public List<PeriodQuantityDto> HistoricalWeeklySales { get; set; } = new();
}
