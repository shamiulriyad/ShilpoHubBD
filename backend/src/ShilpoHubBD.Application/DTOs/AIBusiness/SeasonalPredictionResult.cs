namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class SeasonalPredictionResult
{
    public List<MonthDemandScoreDto> MonthlyScores { get; set; } = new();
    public string PeakSeason { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}
