namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class MonthDemandScoreDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal DemandScore { get; set; }
}
