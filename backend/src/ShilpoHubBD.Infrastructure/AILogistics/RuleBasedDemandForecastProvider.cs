using System.Text.Json;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Infrastructure.AILogistics;

/// <summary>
/// Rule-based stand-in for a demand-forecasting model. Fits an OLS line to the daily history,
/// derives day-of-week multipliers from the residual ratios, projects the trend forward with that
/// seasonality and widens the band with the horizon. Weekly granularity sums the daily projection
/// into 7-day buckets. No external calls, no weights.
/// </summary>
public class RuleBasedDemandForecastProvider : IDemandForecastProvider
{
    public const string Name = "rule-based-demand-forecast-v1";

    public string ProviderName => Name;

    public LogisticsDemandForecastResult Forecast(LogisticsDemandForecastInput input)
    {
        var horizon = Math.Clamp(input.HorizonDays, 1, 180);
        var history = input.History.OrderBy(h => h.Date).ToList();
        var baseline = history.Count > 0 ? history.Average(h => h.Value) : 0.0;

        double slope = 0;
        double intercept = baseline;
        double r2 = 0;
        if (history.Count >= 4)
        {
            var xs = Enumerable.Range(0, history.Count).Select(i => (double)i).ToArray();
            var ys = history.Select(h => h.Value).ToArray();
            var meanX = xs.Average();
            var meanY = ys.Average();
            var sxx = xs.Sum(x => (x - meanX) * (x - meanX));
            var sxy = xs.Zip(ys, (x, y) => (x - meanX) * (y - meanY)).Sum();
            slope = sxx == 0 ? 0 : sxy / sxx;
            intercept = meanY - slope * meanX;
            var residuals = xs.Zip(ys, (x, y) => y - (intercept + slope * x)).ToArray();
            var totalSs = ys.Sum(y => (y - meanY) * (y - meanY));
            r2 = totalSs == 0 ? 1 : 1 - residuals.Sum(r => r * r) / totalSs;
        }

        var dowMultiplier = DayOfWeekMultipliers(history, intercept, slope);

        var dailyStd = history.Count >= 2
            ? Math.Sqrt(history.Select((h, i) => h.Value - (intercept + slope * i))
                .Select(r => r * r).Sum() / Math.Max(1, history.Count - 2))
            : Math.Max(1.0, baseline * 0.3);

        var startDate = input.AsOf.Date.AddDays(1);
        var dailyProjection = new List<(DateTime Date, double Value, double Lower, double Upper)>(horizon);
        for (var d = 0; d < horizon; d++)
        {
            var date = startDate.AddDays(d);
            var trend = Math.Max(0, intercept + slope * (history.Count + d));
            var seasonal = trend * dowMultiplier[(int)date.DayOfWeek];
            var band = Math.Max(dailyStd, seasonal * 0.1) * (1 + 0.6 * (d + 1) / horizon);
            dailyProjection.Add((date, seasonal, Math.Max(0, seasonal - band), seasonal + band));
        }

        var weekly = input.Granularity.Equals("week", StringComparison.OrdinalIgnoreCase);
        var points = weekly ? Bucketize(dailyProjection, 7) : dailyProjection;

        var predictedTotal = dailyProjection.Sum(p => p.Value);

        var confidence = (history.Count, r2) switch
        {
            ( >= 30, >= 0.5) => AiLogisticsConfidence.High,
            ( >= 10, >= 0.2) => AiLogisticsConfidence.Moderate,
            _ => AiLogisticsConfidence.Low,
        };

        var assumptions = JsonSerializer.Serialize(new
        {
            model = "OLS trend on daily history + day-of-week seasonality",
            historyDays = history.Count,
            slopePerDay = Math.Round(slope, 4),
            r2 = Math.Round(r2, 3),
            dowMultipliers = dowMultiplier.Select(m => Math.Round(m, 3)),
            bandRule = "max(daily residual stdev, 10% of value), widening to +60% at the horizon",
        });

        var summary =
            $"Forecast '{input.Metric}' for {horizon} day(s) from {history.Count} day(s) of history: "
            + $"baseline {Math.Round(baseline, 1)}/day, projected total {Math.Round(predictedTotal, 1)}.";

        return new LogisticsDemandForecastResult(
            Name,
            summary,
            assumptions,
            Math.Round(baseline, 3),
            Math.Round(predictedTotal, 2),
            confidence,
            points.Select(p => new LogisticsDemandForecastPointResult(
                p.Date, Math.Round(p.Value, 2), Math.Round(p.Lower, 2), Math.Round(p.Upper, 2))).ToList());
    }

    private static double[] DayOfWeekMultipliers(
        IReadOnlyList<LogisticsDemandObservation> history, double intercept, double slope)
    {
        var sums = new double[7];
        var counts = new int[7];
        for (var i = 0; i < history.Count; i++)
        {
            var trend = intercept + slope * i;
            if (trend <= 0)
            {
                continue;
            }

            var dow = (int)history[i].Date.DayOfWeek;
            sums[dow] += history[i].Value / trend;
            counts[dow]++;
        }

        var result = new double[7];
        for (var d = 0; d < 7; d++)
        {
            result[d] = counts[d] > 0 ? Math.Clamp(sums[d] / counts[d], 0.2, 3.0) : 1.0;
        }

        return result;
    }

    private static List<(DateTime Date, double Value, double Lower, double Upper)> Bucketize(
        List<(DateTime Date, double Value, double Lower, double Upper)> daily, int size)
    {
        var buckets = new List<(DateTime, double, double, double)>();
        for (var i = 0; i < daily.Count; i += size)
        {
            var slice = daily.Skip(i).Take(size).ToList();
            buckets.Add((
                slice[0].Date,
                slice.Sum(s => s.Value),
                slice.Sum(s => s.Lower),
                slice.Sum(s => s.Upper)));
        }

        return buckets;
    }
}
