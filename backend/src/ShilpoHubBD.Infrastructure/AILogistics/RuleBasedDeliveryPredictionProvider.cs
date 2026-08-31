using System.Text.Json;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Infrastructure.AILogistics;

/// <summary>
/// Rule-based stand-in for a delivery-prediction model. Starts from a service-level transit baseline,
/// blends in the lane's historical transit time, then nudges the on-time and failure odds with the
/// lane's and the partner's recent performance, the current status and the attempt count. No
/// external calls, no weights. Swap for a real <see cref="IDeliveryPredictionProvider"/> later.
/// </summary>
public class RuleBasedDeliveryPredictionProvider : IDeliveryPredictionProvider
{
    public const string Name = "rule-based-delivery-prediction-v1";

    public string ProviderName => Name;

    public LogisticsDeliveryPredictionResult Predict(LogisticsDeliveryPredictionInput input)
    {
        var serviceBaseline = input.ServiceLevel.ToLowerInvariant() switch
        {
            "sameday" => 0.5,
            "express" => 1.5,
            "standard" => 3.0,
            "economy" => 5.0,
            _ => 3.0,
        };

        if (input.SameDistrict)
        {
            serviceBaseline *= 0.7;
        }

        // Blend service baseline with the observed lane average (weight grows with sample size).
        var laneWeight = input.HistoricalTransitDaysAvg.HasValue
            ? Math.Clamp(input.LaneSampleSize / 20.0, 0.0, 0.8)
            : 0.0;
        var transitDays = laneWeight > 0
            ? serviceBaseline * (1 - laneWeight) + input.HistoricalTransitDaysAvg!.Value * laneWeight
            : serviceBaseline;

        // Status progress shortens the remaining estimate.
        var remainingFactor = input.CurrentStatus.ToLowerInvariant() switch
        {
            "outfordelivery" => 0.1,
            "athub" => 0.45,
            "intransit" => 0.55,
            "pickedup" => 0.8,
            "deliveryfailed" => 0.6,
            _ => 1.0,
        };

        var anchor = input.DispatchedAt ?? input.NowUtc;
        var predictedDeliveryAt = anchor.AddDays(transitDays * remainingFactor).AddHours(4);
        if (predictedDeliveryAt < input.NowUtc)
        {
            predictedDeliveryAt = input.NowUtc.AddHours(6);
        }

        // On-time score around a logistic curve.
        var laneOnTime = input.HistoricalOnTimeRate ?? 0.85;
        var partnerOnTime = input.PartnerOnTimeRate ?? laneOnTime;
        var score = 0.55 * laneOnTime + 0.35 * partnerOnTime + 0.10 * 0.85;

        if (input.PromisedDeliveryAt.HasValue && predictedDeliveryAt > input.PromisedDeliveryAt.Value)
        {
            var lateDays = (predictedDeliveryAt - input.PromisedDeliveryAt.Value).TotalDays;
            score -= Math.Clamp(lateDays * 0.18, 0.0, 0.45);
        }

        score -= input.DeliveryAttemptCount * 0.12;
        if (input.IsCashOnDelivery)
        {
            score -= 0.04;
        }

        if (input.CurrentStatus.Equals("DeliveryFailed", StringComparison.OrdinalIgnoreCase))
        {
            score -= 0.15;
        }

        var onTimeProbability = Math.Clamp(score, 0.02, 0.99);

        var laneFailure = input.HistoricalFailureRate ?? 0.05;
        var failureProbability = Math.Clamp(
            laneFailure + input.DeliveryAttemptCount * 0.08 + (input.IsCashOnDelivery ? 0.03 : 0.0),
            0.0,
            0.9);

        var risk = (onTimeProbability, failureProbability) switch
        {
            ( >= 0.85, < 0.1) => DeliveryRiskLevel.Low,
            ( >= 0.6, < 0.25) => DeliveryRiskLevel.Moderate,
            ( >= 0.35, _) => DeliveryRiskLevel.High,
            _ => DeliveryRiskLevel.Severe,
        };

        var confidence = input.LaneSampleSize switch
        {
            >= 25 => AiLogisticsConfidence.High,
            >= 8 => AiLogisticsConfidence.Moderate,
            _ => AiLogisticsConfidence.Low,
        };

        var factors = JsonSerializer.Serialize(new
        {
            serviceBaselineDays = Math.Round(serviceBaseline, 2),
            laneAverageDays = input.HistoricalTransitDaysAvg,
            laneWeight = Math.Round(laneWeight, 2),
            laneSampleSize = input.LaneSampleSize,
            blendedTransitDays = Math.Round(transitDays, 2),
            remainingFactor,
            laneOnTimeRate = Math.Round(laneOnTime, 3),
            partnerOnTimeRate = Math.Round(partnerOnTime, 3),
            deliveryAttempts = input.DeliveryAttemptCount,
            isCashOnDelivery = input.IsCashOnDelivery,
            currentStatus = input.CurrentStatus,
        });

        var summary =
            $"Predicted delivery around {predictedDeliveryAt:yyyy-MM-dd} "
            + $"(~{Math.Round(transitDays * remainingFactor, 1)} day(s) remaining); "
            + $"on-time {Math.Round(onTimeProbability * 100)}%, failure {Math.Round(failureProbability * 100)}%, risk {risk}.";

        return new LogisticsDeliveryPredictionResult(
            Name,
            Math.Round(transitDays, 2),
            predictedDeliveryAt,
            Math.Round(onTimeProbability, 4),
            Math.Round(failureProbability, 4),
            risk,
            confidence,
            summary,
            factors);
    }
}
