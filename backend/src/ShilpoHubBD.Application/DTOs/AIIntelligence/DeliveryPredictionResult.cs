namespace ShilpoHubBD.Application.DTOs.AIIntelligence;

public class DeliveryPredictionResult
{
    public double PredictedDeliveryDays { get; set; }
    public string ConfidenceLevel { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
}
