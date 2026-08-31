using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>
/// A rule-based prediction of when a <see cref="Shipment"/> will be delivered and how likely it is to
/// be on time / fail, produced by the pluggable delivery-prediction provider. No real model.
/// </summary>
public class DeliveryPrediction
{
    public Guid Id { get; set; }

    public Guid LogisticsPartnerProfileId { get; set; }
    public LogisticsPartnerProfile Profile { get; set; } = null!;

    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;

    public Guid GeneratedByUserId { get; set; }
    public User GeneratedBy { get; set; } = null!;

    public string Method { get; set; } = string.Empty;

    public DateTime? PredictedDeliveryAt { get; set; }
    public double PredictedTransitDays { get; set; }
    public double OnTimeProbability { get; set; }
    public double PredictedFailureProbability { get; set; }

    public DeliveryRiskLevel RiskLevel { get; set; }
    public AiLogisticsConfidence Confidence { get; set; }

    public string Summary { get; set; } = string.Empty;

    /// <summary>JSON: the signals and per-factor impacts that drove the prediction.</summary>
    public string? FactorsJson { get; set; }

    public DateTime CreatedAt { get; set; }
}
