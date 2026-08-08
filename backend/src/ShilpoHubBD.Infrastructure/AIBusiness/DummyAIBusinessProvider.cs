using ShilpoHubBD.Application.DTOs.AIBusiness;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Infrastructure.AIBusiness;

// Rule-based stand-in for a future AI/ML backend (Gemini, OpenAI, custom model, etc). Every method
// derives its answer from simple heuristics over the context it is given -- no external calls, no
// model weights. Swap this out for a real implementation of IAIBusinessProvider later; nothing in
// AIBusinessService or the controller needs to change.
public class DummyAIBusinessProvider : IAIBusinessProvider
{
    private static readonly string[] MonthNames =
    {
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December",
    };

    // Generic Bangladeshi handicraft/gift demand curve (0-100), used when a producer/category
    // doesn't yet have enough order history to derive a real seasonal pattern. Peaks around
    // Pohela Boishakh (April) and the Puja/wedding season (Sep-Dec).
    private static readonly decimal[] FallbackSeasonalIndex =
        { 65, 60, 70, 95, 55, 58, 62, 75, 82, 88, 78, 80 };

    public Task<PriceSuggestionResult> SuggestPriceAsync(PriceSuggestionContext context, CancellationToken cancellationToken)
    {
        var signals = new List<(decimal Price, decimal Weight, string Label)>();

        if (context.CurrentPrice is > 0)
        {
            signals.Add((context.CurrentPrice.Value, 1.0m, "current listed price"));
        }

        if (context.CategoryAveragePrice is > 0)
        {
            signals.Add((context.CategoryAveragePrice.Value, 1.5m, $"average price in {context.CategoryName}"));
        }

        if (context.ProducerAveragePrice is > 0)
        {
            signals.Add((context.ProducerAveragePrice.Value, 1.0m, "your own average product price"));
        }

        if (context.EstimatedCost is > 0)
        {
            var margin = context.DesiredMarginPercent is > 0 ? context.DesiredMarginPercent.Value : 40m;
            var costBased = context.EstimatedCost.Value * (1 + margin / 100m);
            signals.Add((costBased, 2.0m, $"estimated cost with a {margin:0.#}% margin"));
        }

        decimal suggested;
        string rationale;

        if (signals.Count == 0)
        {
            suggested = 500m;
            rationale = "No pricing signals were available, so a generic starter price was used. " +
                "Provide an estimated cost or select a category with existing products for a better suggestion.";
        }
        else
        {
            var weightedSum = signals.Sum(s => s.Price * s.Weight);
            var totalWeight = signals.Sum(s => s.Weight);
            suggested = weightedSum / totalWeight;
            rationale = $"Based on {string.Join(", ", signals.Select(s => s.Label))}.";
        }

        suggested = RoundToNearest(suggested, 10);

        return Task.FromResult(new PriceSuggestionResult
        {
            SuggestedPrice = suggested,
            MinPrice = RoundToNearest(suggested * 0.85m, 10),
            MaxPrice = RoundToNearest(suggested * 1.15m, 10),
            Rationale = rationale,
        });
    }

    public Task<ProductDescriptionResult> GenerateDescriptionAsync(ProductDescriptionContext context, CancellationToken cancellationToken)
    {
        var adjectives = ToneAdjectives(context.Tone);
        var origin = string.IsNullOrWhiteSpace(context.DistrictName) ? "Bangladesh" : context.DistrictName;
        var category = string.IsNullOrWhiteSpace(context.CategoryName) ? "handmade craft" : context.CategoryName;
        var keywordPhrase = context.Keywords.Count > 0 ? $" featuring {string.Join(", ", context.Keywords)}" : string.Empty;

        var description =
            $"Discover the {adjectives[0]} \"{context.ProductName}\", a {adjectives[1]} {category} handcrafted by " +
            $"skilled artisans in {origin}{keywordPhrase}. Each piece carries the {adjectives[2]} touch of " +
            "traditional craftsmanship, making it a meaningful addition to your home or a thoughtful gift.";

        var tags = new List<string> { category, origin, "handmade", "Bangladeshi craft" };
        tags.AddRange(context.Keywords.Take(5));

        return Task.FromResult(new ProductDescriptionResult
        {
            Description = description,
            SuggestedTags = tags.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
        });
    }

    public Task<BusinessTranslationResult> TranslateAsync(BusinessTranslationRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new BusinessTranslationResult
        {
            OriginalText = request.Text,
            TranslatedText = $"[{request.TargetLanguage}] {request.Text}",
            TargetLanguage = request.TargetLanguage,
        });
    }

    public Task<DemandForecastResult> ForecastDemandAsync(DemandForecastContext context, CancellationToken cancellationToken)
    {
        var history = context.HistoricalWeeklySales;
        var forecast = new List<PeriodQuantityDto>();

        if (history.Count == 0)
        {
            var start = GetWeekStart(DateTime.UtcNow).AddDays(7);
            for (var i = 0; i < context.HorizonWeeks; i++)
            {
                forecast.Add(new PeriodQuantityDto { PeriodStart = start.AddDays(7 * i), Quantity = 0 });
            }

            return Task.FromResult(new DemandForecastResult
            {
                ForecastedWeeklyDemand = forecast,
                Trend = "Insufficient data",
                Recommendation = "Not enough order history for this product yet. Forecasts will improve as more orders come in.",
            });
        }

        var average = history.Average(h => h.Quantity);
        var slope = LinearSlope(history.Select(h => (double)h.Quantity).ToList());

        var trend = slope switch
        {
            > 0.3 => "Increasing",
            < -0.3 => "Decreasing",
            _ => "Stable",
        };

        var lastWeekStart = history[^1].PeriodStart;
        for (var i = 1; i <= context.HorizonWeeks; i++)
        {
            var projected = average + slope * (history.Count + i - 1);
            forecast.Add(new PeriodQuantityDto
            {
                PeriodStart = lastWeekStart.AddDays(7 * i),
                Quantity = Math.Max(0, (int)Math.Round(projected)),
            });
        }

        var totalForecast = forecast.Sum(f => f.Quantity);
        var recommendation = totalForecast > context.CurrentStock
            ? $"Projected demand over the next {context.HorizonWeeks} week(s) is {totalForecast} units, which exceeds your " +
              $"current stock of {context.CurrentStock}. Consider planning a production run."
            : $"Current stock ({context.CurrentStock} units) should cover the projected demand of {totalForecast} units " +
              $"over the next {context.HorizonWeeks} week(s).";

        return Task.FromResult(new DemandForecastResult
        {
            ForecastedWeeklyDemand = forecast,
            Trend = trend,
            Recommendation = recommendation,
        });
    }

    public Task<ProductionPlanResult> PlanProductionAsync(ProductionPlannerContext context, CancellationToken cancellationToken)
    {
        var unitsToProduce = Math.Max(0, context.TargetQuantity - context.CurrentStock);
        var startDate = DateTime.UtcNow.Date.AddDays(context.LeadTimeDays);
        var schedule = new List<ProductionScheduleEntryDto>();

        if (unitsToProduce == 0)
        {
            return Task.FromResult(new ProductionPlanResult
            {
                UnitsToProduce = 0,
                EstimatedDaysNeeded = 0,
                RecommendedStartDate = startDate,
                EstimatedCompletionDate = startDate,
                Schedule = schedule,
                Notes = "Current stock already meets or exceeds the target quantity. No production needed.",
            });
        }

        if (context.DailyProductionCapacity <= 0)
        {
            return Task.FromResult(new ProductionPlanResult
            {
                UnitsToProduce = unitsToProduce,
                EstimatedDaysNeeded = 0,
                RecommendedStartDate = startDate,
                EstimatedCompletionDate = startDate,
                Schedule = schedule,
                Notes = "Set a daily production capacity greater than zero to generate a day-by-day schedule.",
            });
        }

        var daysNeeded = (int)Math.Ceiling(unitsToProduce / (double)context.DailyProductionCapacity);
        var cumulative = 0;
        var maxScheduleEntries = Math.Min(daysNeeded, 60);

        for (var day = 0; day < maxScheduleEntries; day++)
        {
            var plannedUnits = Math.Min(context.DailyProductionCapacity, unitsToProduce - cumulative);
            cumulative += plannedUnits;
            schedule.Add(new ProductionScheduleEntryDto
            {
                Date = startDate.AddDays(day),
                PlannedUnits = plannedUnits,
                CumulativeUnits = cumulative,
            });
        }

        return Task.FromResult(new ProductionPlanResult
        {
            UnitsToProduce = unitsToProduce,
            EstimatedDaysNeeded = daysNeeded,
            RecommendedStartDate = startDate,
            EstimatedCompletionDate = startDate.AddDays(Math.Max(0, daysNeeded - 1)),
            Schedule = schedule,
            Notes = $"Producing {context.ProductName}: {unitsToProduce} units needed at {context.DailyProductionCapacity} units/day.",
        });
    }

    public Task<MaterialForecastResult> ForecastMaterialsAsync(MaterialForecastRequest request, CancellationToken cancellationToken)
    {
        var requirements = request.MaterialsPerUnit
            .Select(m =>
            {
                var total = m.QuantityPerUnit * request.UnitsToProduce;
                return new MaterialRequirementDto
                {
                    MaterialName = m.MaterialName,
                    TotalQuantityNeeded = total,
                    RecommendedBufferQuantity = Math.Round(total * 0.1m, 2),
                    Unit = m.Unit,
                };
            })
            .ToList();

        return Task.FromResult(new MaterialForecastResult
        {
            Requirements = requirements,
            Notes = "Quantities include a 10% safety buffer to absorb wastage and material defects.",
        });
    }

    public Task<SeasonalPredictionResult> PredictSeasonalTrendAsync(SeasonalPredictionContext context, CancellationToken cancellationToken)
    {
        var monthlyTotals = context.HistoricalMonthlySales
            .GroupBy(p => p.PeriodStart.Month)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Quantity));

        List<MonthDemandScoreDto> scores;
        bool usedActualData;

        if (monthlyTotals.Count >= 3)
        {
            var max = monthlyTotals.Values.Max();
            scores = Enumerable.Range(1, 12)
                .Select(month => new MonthDemandScoreDto
                {
                    Month = month,
                    MonthName = MonthNames[month - 1],
                    DemandScore = monthlyTotals.TryGetValue(month, out var qty) && max > 0
                        ? Math.Round(qty / (decimal)max * 100m, 1)
                        : 0m,
                })
                .ToList();
            usedActualData = true;
        }
        else
        {
            scores = Enumerable.Range(1, 12)
                .Select(month => new MonthDemandScoreDto
                {
                    Month = month,
                    MonthName = MonthNames[month - 1],
                    DemandScore = FallbackSeasonalIndex[month - 1],
                })
                .ToList();
            usedActualData = false;
        }

        var peak = scores.OrderByDescending(s => s.DemandScore).First();
        var recommendation = usedActualData
            ? $"Based on your order history, {context.CategoryName} demand peaks around {peak.MonthName}. " +
              "Plan stock and marketing pushes ahead of that month."
            : $"Not enough order history yet for {context.CategoryName}, so this uses a general Bangladeshi " +
              $"handicraft demand pattern. Expect the strongest demand around {peak.MonthName} " +
              "(festive/wedding season). This will refine as more orders come in.";

        return Task.FromResult(new SeasonalPredictionResult
        {
            MonthlyScores = scores,
            PeakSeason = peak.MonthName,
            Recommendation = recommendation,
        });
    }

    public Task<SalesInsightsResult> GenerateSalesInsightsAsync(SalesInsightsContext context, CancellationToken cancellationToken)
    {
        var insights = new List<string>();
        var revenue = context.Revenue;
        var sales = context.Sales;

        if (revenue.TotalOrders == 0)
        {
            insights.Add("No completed sales yet. Insights will appear once orders start coming in.");
        }
        else
        {
            insights.Add(
                $"Total revenue is ৳{revenue.TotalRevenue:N0} from {revenue.TotalOrders} order(s), " +
                $"averaging ৳{revenue.AverageOrderValue:N0} per order.");
        }

        if (revenue.PendingCount > 0)
        {
            insights.Add($"You have {revenue.PendingCount} pending order(s) awaiting acceptance or rejection.");
        }

        if (sales.DailySales.Count >= 4)
        {
            var midpoint = sales.DailySales.Count / 2;
            var firstHalfAvg = sales.DailySales.Take(midpoint).Average(d => d.Revenue);
            var secondHalfAvg = sales.DailySales.Skip(midpoint).Average(d => d.Revenue);

            if (firstHalfAvg > 0)
            {
                var changePercent = (secondHalfAvg - firstHalfAvg) / firstHalfAvg * 100m;
                if (Math.Abs(changePercent) >= 5)
                {
                    var direction = changePercent > 0 ? "up" : "down";
                    insights.Add($"Daily revenue is trending {direction} {Math.Abs(changePercent):0.#}% compared to the earlier part of this period.");
                }
            }
        }

        var topProduct = sales.TopProducts.FirstOrDefault();
        if (topProduct is not null)
        {
            insights.Add($"Your best-selling product is \"{topProduct.ProductName}\", generating ৳{topProduct.Revenue:N0} in revenue.");
        }

        var lowConversion = context.ProductPerformance
            .Where(p => p.ViewCount >= 20 && p.SalesCount == 0)
            .OrderByDescending(p => p.ViewCount)
            .FirstOrDefault();

        if (lowConversion is not null)
        {
            insights.Add(
                $"\"{lowConversion.ProductName}\" has {lowConversion.ViewCount} views but no sales yet -- " +
                "consider adjusting the price, photos, or description.");
        }

        if (insights.Count == 0)
        {
            insights.Add("Add products and gather order history to start receiving sales insights.");
        }

        return Task.FromResult(new SalesInsightsResult { Insights = insights });
    }

    private static string[] ToneAdjectives(string tone) => tone.Trim().ToLowerInvariant() switch
    {
        "premium" => new[] { "exquisite", "refined", "luxurious" },
        "rustic" => new[] { "earthy", "rustic", "time-honored" },
        "playful" => new[] { "charming", "playful", "delightful" },
        _ => new[] { "heartfelt", "lovingly crafted", "authentic" },
    };

    private static decimal RoundToNearest(decimal value, int nearest)
        => Math.Round(value / nearest, MidpointRounding.AwayFromZero) * nearest;

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

    private static DateTime GetWeekStart(DateTime date)
    {
        date = date.Date;
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }
}
