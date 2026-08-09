using ShilpoHubBD.Application.DTOs.AIIntelligence;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Infrastructure.AIBusinessPartner;

// Rule-based stand-in for a future AI/ML backend (Gemini, OpenAI, custom model, etc). Every method
// derives its answer from simple heuristics over the context it is given -- no external calls, no
// model weights. Swap this out for a real implementation of IAIBusinessPartnerProvider later;
// nothing in BusinessPartnerAIService or the controller needs to change.
public class DummyBusinessPartnerAIProvider : IAIBusinessPartnerProvider
{
    public Task<SupplierRankingResult> RankSuppliersAsync(SupplierRankingContext context, CancellationToken cancellationToken)
    {
        var rankings = context.Candidates
            .Select(c =>
            {
                var ratingScore = Math.Min(100m, c.AverageRating / 5m * 100m);
                var certificationScore = Math.Min(100m, c.CertificationCount * 25m);
                var capacityScore = Math.Min(100m, c.EstimatedProductionCapacity / 10m);
                var verifiedBonus = c.IsHandmadeVerified ? 100m : 0m;

                var rankScore = Math.Round(
                    (ratingScore * 0.4m) + (certificationScore * 0.2m) + (capacityScore * 0.2m) + (verifiedBonus * 0.1m) +
                    (Math.Min(1m, c.ReviewCount / 20m) * 100m * 0.1m), 1);

                var confidence = Math.Round(Math.Min(1m, 0.3m + (c.ReviewCount / 20m * 0.5m) + (c.CertificationCount > 0 ? 0.2m : 0m)), 2);

                var reasons = new List<string>();
                if (c.ReviewCount > 0)
                {
                    reasons.Add($"{c.AverageRating:0.0}/5 rating from {c.ReviewCount} review(s)");
                }

                if (c.CertificationCount > 0)
                {
                    reasons.Add($"{c.CertificationCount} certification(s)");
                }

                if (c.IsHandmadeVerified)
                {
                    reasons.Add("handmade-verified products");
                }

                reasons.Add($"~{c.EstimatedProductionCapacity} unit(s) estimated capacity");

                return new SupplierRankingEntryDto
                {
                    ProducerId = c.ProducerId,
                    ProducerName = c.ProducerName,
                    RankScore = rankScore,
                    Confidence = confidence,
                    Reasoning = "Ranked on " + string.Join(", ", reasons) + ".",
                };
            })
            .OrderByDescending(r => r.RankScore)
            .ToList();

        return Task.FromResult(new SupplierRankingResult { Rankings = rankings });
    }

    public Task<QualityPredictionResult> PredictQualityAsync(QualityPredictionContext context, CancellationToken cancellationToken)
    {
        var ratingScore = Math.Min(100m, context.AverageRating / 5m * 100m);
        var verifiedRatio = context.ProductCount == 0
            ? 0m
            : Math.Min(1m, (decimal)context.HandmadeVerifiedProductCount / context.ProductCount);

        var totalFulfilled = context.DeliveredOrderItemCount + context.CancelledOrderItemCount;
        var deliveryReliability = totalFulfilled == 0
            ? 0.5m // neutral -- no fulfillment history yet
            : (decimal)context.DeliveredOrderItemCount / totalFulfilled;

        var score = Math.Round((ratingScore * 0.5m) + (verifiedRatio * 100m * 0.25m) + (deliveryReliability * 100m * 0.25m), 1);

        var tier = score switch
        {
            >= 80 => "Excellent",
            >= 60 => "Good",
            >= 40 => "Average",
            _ => "NeedsImprovement",
        };

        var reasoning = context.ReviewCount == 0
            ? $"No reviews yet for {context.ProducerName}; this prediction is based on handmade verification and fulfillment history only."
            : $"Based on a {context.AverageRating:0.0}/5 rating across {context.ReviewCount} review(s), " +
              $"{context.HandmadeVerifiedProductCount} of {context.ProductCount} product(s) handmade-verified, " +
              $"and {context.DeliveredOrderItemCount} delivered vs {context.CancelledOrderItemCount} cancelled order item(s).";

        return Task.FromResult(new QualityPredictionResult
        {
            PredictedQualityScore = score,
            QualityTier = tier,
            Reasoning = reasoning,
        });
    }

    public Task<PriceForecastResult> ForecastPriceAsync(PriceForecastContext context, CancellationToken cancellationToken)
    {
        var history = context.HistoricalMonthlyAveragePrice;
        var forecast = new List<PeriodPriceDto>();

        if (history.Count == 0)
        {
            return Task.FromResult(new PriceForecastResult
            {
                ForecastedPrices = forecast,
                Trend = "Insufficient data",
                Recommendation = $"Not enough completed order history for {context.CategoryName} yet to forecast price trends.",
            });
        }

        var average = history.Average(h => h.AveragePrice);
        var slope = LinearSlope(history.Select(h => (double)h.AveragePrice).ToList());

        var trend = slope switch
        {
            > 0.5 => "Increasing",
            < -0.5 => "Decreasing",
            _ => "Stable",
        };

        var lastPeriod = history[^1].PeriodStart;
        for (var i = 1; i <= context.HorizonMonths; i++)
        {
            var projected = average + ((decimal)slope * (history.Count + i - 1));
            forecast.Add(new PeriodPriceDto
            {
                PeriodStart = new DateTime(lastPeriod.Year, lastPeriod.Month, 1).AddMonths(i),
                AveragePrice = Math.Max(0, Math.Round(projected, 2)),
            });
        }

        var recommendation = trend switch
        {
            "Increasing" => $"{context.CategoryName} prices have been trending up. Consider locking in pricing with long-term contracts sooner rather than later.",
            "Decreasing" => $"{context.CategoryName} prices have been trending down. You may benefit from waiting or negotiating on upcoming purchases.",
            _ => $"{context.CategoryName} prices have been stable. No urgency to change your procurement timing.",
        };

        return Task.FromResult(new PriceForecastResult
        {
            ForecastedPrices = forecast,
            Trend = trend,
            Recommendation = recommendation,
        });
    }

    public Task<DeliveryPredictionResult> PredictDeliveryAsync(DeliveryPredictionContext context, CancellationToken cancellationToken)
    {
        if (context.HistoricalDeliveryDays.Count == 0)
        {
            return Task.FromResult(new DeliveryPredictionResult
            {
                PredictedDeliveryDays = 14,
                ConfidenceLevel = "Low",
                Reasoning = $"{context.ProducerName} has no completed delivery history yet; using a generic 14-day estimate.",
            });
        }

        var average = context.HistoricalDeliveryDays.Average();

        // Scale up the estimate when the requested quantity exceeds the producer's current
        // estimated capacity, since a larger order likely needs more than one production cycle.
        if (context.RequestedQuantity.HasValue && context.EstimatedProductionCapacity > 0
            && context.RequestedQuantity.Value > context.EstimatedProductionCapacity)
        {
            var cycles = Math.Ceiling(context.RequestedQuantity.Value / (double)context.EstimatedProductionCapacity);
            average *= cycles;
        }

        var confidence = context.HistoricalDeliveryDays.Count switch
        {
            >= 10 => "High",
            >= 3 => "Medium",
            _ => "Low",
        };

        var reasoning = $"Based on {context.HistoricalDeliveryDays.Count} past delivered order(s) averaging " +
            $"{context.HistoricalDeliveryDays.Average():0.#} day(s) from order to delivery" +
            (context.RequestedQuantity.HasValue ? $", adjusted for the requested quantity of {context.RequestedQuantity.Value}." : ".");

        return Task.FromResult(new DeliveryPredictionResult
        {
            PredictedDeliveryDays = Math.Round(average, 1),
            ConfidenceLevel = confidence,
            Reasoning = reasoning,
        });
    }

    public Task<RiskAssessmentResult> AssessRiskAsync(RiskAssessmentContext context, CancellationToken cancellationToken)
    {
        var riskFactors = new List<string>();
        decimal riskScore = 0;

        if (context.ReviewCount > 0 && context.AverageRating < 3.5m)
        {
            riskScore += 25;
            riskFactors.Add($"Below-average rating ({context.AverageRating:0.0}/5).");
        }

        if (context.TotalOrderItemCount > 0)
        {
            var cancellationRate = (decimal)context.CancelledOrderItemCount / context.TotalOrderItemCount;
            if (cancellationRate > 0.1m)
            {
                riskScore += Math.Min(25m, cancellationRate * 100m);
                riskFactors.Add($"{cancellationRate:P0} of past order items were cancelled.");
            }
        }

        if (context.TotalQuotationResponseCount > 0)
        {
            var rejectionRate = (decimal)context.RejectedQuotationResponseCount / context.TotalQuotationResponseCount;
            if (rejectionRate > 0.3m)
            {
                riskScore += Math.Min(20m, rejectionRate * 60m);
                riskFactors.Add($"{rejectionRate:P0} of submitted quotations were rejected.");
            }
        }

        if (context.TotalProcurementCount > 0)
        {
            var procurementCancellationRate = (decimal)context.CancelledProcurementCount / context.TotalProcurementCount;
            if (procurementCancellationRate > 0.1m)
            {
                riskScore += Math.Min(15m, procurementCancellationRate * 50m);
                riskFactors.Add($"{procurementCancellationRate:P0} of procurement requests were cancelled.");
            }
        }

        if (!context.HasVerifiedCertification)
        {
            riskScore += 10;
            riskFactors.Add("No verified certifications on file.");
        }

        if (context.ReviewCount == 0 && context.TotalOrderItemCount == 0)
        {
            riskScore += 15;
            riskFactors.Add("No order or review history yet -- limited track record.");
        }

        riskScore = Math.Min(100m, Math.Round(riskScore, 1));

        var riskLevel = riskScore switch
        {
            >= 60 => "High",
            >= 30 => "Medium",
            _ => "Low",
        };

        if (riskFactors.Count == 0)
        {
            riskFactors.Add("No significant risk factors identified.");
        }

        var recommendation = riskLevel switch
        {
            "High" => $"Proceed cautiously with {context.ProducerName} -- consider smaller trial orders or requesting additional guarantees.",
            "Medium" => $"{context.ProducerName} carries some risk; review the factors below before committing to large orders.",
            _ => $"{context.ProducerName} shows a solid track record with no major risk indicators.",
        };

        return Task.FromResult(new RiskAssessmentResult
        {
            RiskScore = riskScore,
            RiskLevel = riskLevel,
            RiskFactors = riskFactors,
            Recommendation = recommendation,
        });
    }

    private static double LinearSlope(List<double> values)
    {
        var n = values.Count;
        if (n < 2)
        {
            return 0;
        }

        var xMean = (n - 1) / 2.0;
        var yMean = values.Average();

        double numerator = 0;
        double denominator = 0;
        for (var i = 0; i < n; i++)
        {
            numerator += (i - xMean) * (values[i] - yMean);
            denominator += (i - xMean) * (i - xMean);
        }

        return denominator == 0 ? 0 : numerator / denominator;
    }
}
