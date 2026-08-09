namespace ShilpoHubBD.Application.DTOs.AIIntelligence;

public class PriceForecastResult
{
    public List<PeriodPriceDto> ForecastedPrices { get; set; } = new();
    public string Trend { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}
