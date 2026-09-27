using ShilpoHubBD.Application.DTOs.ProductIntelligence;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Infrastructure.ProductIntelligence;

/// <summary>
/// Deterministic, rule-based fallback used when Gemini has no API key configured or a call fails —
/// every sentence is templated directly from the supplied aggregated numbers, never invented.
/// </summary>
public class DummyProductIntelligenceAIProvider : IProductIntelligenceAIProvider
{
    public Task<ProductIntelligenceAiInsightsDto> GenerateInsightsAsync(ProductIntelligenceAiContext context, CancellationToken cancellationToken)
    {
        if (!context.HasSufficientHistory)
        {
            return Task.FromResult(new ProductIntelligenceAiInsightsDto
            {
                DemandTrend = "Insufficient data",
                EstimatedNextPeriodDemand = "Not enough order history in this range to estimate.",
                SalesTrendInterpretation = $"\"{context.ProductName}\" has no delivered orders in the selected period ({context.RangeLabel}).",
                InventoryRecommendation = context.CurrentStock > 0
                    ? $"Current stock is {context.CurrentStock} units with no recent sales to size demand against — hold at current levels until sales history builds up."
                    : "No stock and no sales history — list stock before drawing any demand conclusions.",
                PricingObservation = "Not enough sales history to relate price to demand yet.",
                MarketingOpportunities = new List<string> { "Consider running a launch promotion to generate initial sales and review data." },
                RiskIndicators = new List<string> { "No sales history — demand is unproven." },
                IsAiGenerated = false,
                GeneratedAt = DateTime.UtcNow,
            });
        }

        var totalUnits = context.Periods.Sum(p => p.UnitsSold);
        var periodCount = Math.Max(1, context.Periods.Count);
        var averageUnitsPerPeriod = totalUnits / (decimal)periodCount;
        var growth = context.UnitsGrowthPercent;

        var demandTrend = growth switch
        {
            null => "Stable (not enough history to compare periods)",
            > 10 => "Increasing",
            < -10 => "Declining",
            _ => "Stable",
        };

        var estimatedNext = Math.Max(0, Math.Round(averageUnitsPerPeriod * (1 + (growth ?? 0) / 100m), 0));

        var riskIndicators = new List<string>();
        var marketingOpportunities = new List<string>();

        if (context.LowStockThreshold.HasValue && context.CurrentStock <= context.LowStockThreshold.Value)
        {
            riskIndicators.Add($"Stock ({context.CurrentStock}) is at or below the low-stock threshold ({context.LowStockThreshold}).");
        }

        if (growth is < -10)
        {
            riskIndicators.Add($"Units sold fell {Math.Abs(growth.Value)}% between the two halves of the selected period.");
        }

        if (context.AverageRating > 0 && context.AverageRating < 3.5m)
        {
            riskIndicators.Add($"Average rating ({context.AverageRating}) is below 3.5 — quality perception may be limiting demand.");
        }

        if (context.TotalReviewCount < 5)
        {
            marketingOpportunities.Add("Encourage recent buyers to leave reviews — low review count limits new-buyer confidence.");
        }

        if (growth is > 10)
        {
            marketingOpportunities.Add("Sales are trending up — consider featuring this product or increasing stock to capture the momentum.");
        }

        if (riskIndicators.Count == 0) riskIndicators.Add("No significant risk indicators found in the current data.");
        if (marketingOpportunities.Count == 0) marketingOpportunities.Add("No specific opportunity stands out from the current data.");

        return Task.FromResult(new ProductIntelligenceAiInsightsDto
        {
            DemandTrend = demandTrend,
            EstimatedNextPeriodDemand = $"Approximately {estimatedNext:N0} units in the next period, extrapolated from the recent average of {averageUnitsPerPeriod:N1} units per period.",
            SalesTrendInterpretation = growth.HasValue
                ? $"Revenue moved {(context.RevenueGrowthPercent >= 0 ? "up" : "down")} {Math.Abs(context.RevenueGrowthPercent ?? 0)}% and units {(growth >= 0 ? "up" : "down")} {Math.Abs(growth.Value)}% between the two halves of {context.RangeLabel}."
                : $"Not enough periods in {context.RangeLabel} to compare a trend.",
            InventoryRecommendation = averageUnitsPerPeriod > 0 && context.CurrentStock < averageUnitsPerPeriod
                ? $"Current stock ({context.CurrentStock}) is below the recent average sales per period ({averageUnitsPerPeriod:N1}) — consider restocking."
                : $"Current stock ({context.CurrentStock}) covers the recent average sales per period ({averageUnitsPerPeriod:N1}).",
            PricingObservation = context.AverageRating >= 4m
                ? "Rating is strong; if sales are flat, price is unlikely to be the limiting factor."
                : "Rating and sales data together should be reviewed before any price change.",
            MarketingOpportunities = marketingOpportunities,
            RiskIndicators = riskIndicators,
            IsAiGenerated = false,
            GeneratedAt = DateTime.UtcNow,
        });
    }
}
