using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>
/// Predicts a shipment's delivery date and its on-time / failure odds. The default implementation is
/// a transparent rule-based heuristic over historical transit stats; the abstraction leaves room for
/// a real model later without touching the service or controller.
/// </summary>
public interface IDeliveryPredictionProvider
{
    string ProviderName { get; }

    LogisticsDeliveryPredictionResult Predict(LogisticsDeliveryPredictionInput input);
}

public record LogisticsDeliveryPredictionInput
{
    public string ServiceLevel { get; init; } = "Standard";
    public string CurrentStatus { get; init; } = "Created";
    public DateTime NowUtc { get; init; }
    public DateTime? DispatchedAt { get; init; }
    public DateTime? PromisedDeliveryAt { get; init; }
    public int DeliveryAttemptCount { get; init; }
    public bool IsCashOnDelivery { get; init; }
    public bool SameDistrict { get; init; }

    public double? HistoricalTransitDaysAvg { get; init; }
    public double? HistoricalOnTimeRate { get; init; }
    public double? HistoricalFailureRate { get; init; }
    public int LaneSampleSize { get; init; }

    public double? PartnerOnTimeRate { get; init; }
    public double? PartnerAvgAttempts { get; init; }
}

public record LogisticsDeliveryPredictionResult(
    string Method,
    double PredictedTransitDays,
    DateTime? PredictedDeliveryAt,
    double OnTimeProbability,
    double PredictedFailureProbability,
    DeliveryRiskLevel RiskLevel,
    AiLogisticsConfidence Confidence,
    string Summary,
    string FactorsJson);
