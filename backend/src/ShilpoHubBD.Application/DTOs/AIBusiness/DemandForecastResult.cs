namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class DemandForecastResult
{
    public List<PeriodQuantityDto> ForecastedWeeklyDemand { get; set; } = new();
    public string Trend { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}
