namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class SeasonalPredictionContext
{
    public string CategoryName { get; set; } = string.Empty;
    public List<PeriodQuantityDto> HistoricalMonthlySales { get; set; } = new();
}
