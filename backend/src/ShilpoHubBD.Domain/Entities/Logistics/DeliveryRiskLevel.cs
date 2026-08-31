namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>How likely a <see cref="DeliveryPrediction"/> thinks a shipment is to miss its promise.</summary>
public enum DeliveryRiskLevel
{
    Low,
    Moderate,
    High,
    Severe,
}
