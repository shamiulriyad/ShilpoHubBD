namespace ShilpoHubBD.Application.DTOs.AIIntelligence;

public class QualityPredictionResult
{
    public decimal PredictedQualityScore { get; set; }
    public string QualityTier { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
}
