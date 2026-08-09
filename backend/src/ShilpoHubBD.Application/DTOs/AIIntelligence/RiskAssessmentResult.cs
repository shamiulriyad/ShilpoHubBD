namespace ShilpoHubBD.Application.DTOs.AIIntelligence;

public class RiskAssessmentResult
{
    public decimal RiskScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public List<string> RiskFactors { get; set; } = new();
    public string Recommendation { get; set; } = string.Empty;
}
