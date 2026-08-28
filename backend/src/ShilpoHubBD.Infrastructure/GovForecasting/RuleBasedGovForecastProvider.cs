using System.Text.Json;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Infrastructure.GovForecasting;

/// <summary>
/// Rule-based stand-in for a future forecasting model behind the Government / NGO "AI Predictions".
/// Fits an ordinary least-squares line to each metric's snapshot history and projects it forward,
/// widening the confidence band with the horizon. No external calls, no model weights. Swap for a
/// real <see cref="IGovForecastProvider"/> later without touching the service or controller.
/// </summary>
public class RuleBasedGovForecastProvider : IGovForecastProvider
{
    public const string Name = "rule-based-gov-forecast-v1";

    private static readonly (string Key, string Unit)[] Metrics =
    {
        ("TotalProducers", "count"),
        ("JobsFilled", "people"),
        ("MarketplaceSalesValue", "BDT"),
        ("ExportSalesValue", "BDT"),
        ("TourismRevenue", "BDT"),
        ("HeritageEconomyValue", "BDT"),
    };

    public string ProviderName => Name;

    public GovForecastResult Forecast(GovForecastInput input)
    {
        var horizon = Math.Clamp(input.HorizonMonths, 1, 60);
        var series = new List<GovForecastSeries>();

        foreach (var (key, unit) in Metrics)
        {
            var baseline = input.CurrentValues.TryGetValue(key, out var cv) ? cv : 0m;

            var observations = input.History
                .Where(h => h.Values.ContainsKey(key))
                .Select(h => (T: h.PeriodEnd, V: (double)h.Values[key]))
                .OrderBy(x => x.T)
                .ToList();

            var projections = observations.Count >= 2
                ? ProjectTrend(observations, baseline, input.BaselineAsOf, horizon)
                : ProjectFlat(baseline, input.BaselineAsOf, horizon, observations.Count);

            series.Add(new GovForecastSeries(key, unit, baseline, projections));
        }

        var assumptions = JsonSerializer.Serialize(new
        {
            model = "ordinary least squares on monthly snapshot history",
            horizonMonths = horizon,
            historyPoints = input.History.Count,
            bandRule = "max(residual stdev, 8% of value), widening linearly to +60% at the horizon",
        });

        return new GovForecastResult(
            Name,
            $"Projected {Metrics.Length} national metrics {horizon} months forward from "
            + $"{input.History.Count} historical snapshot(s) as of {input.BaselineAsOf:yyyy-MM-dd}.",
            assumptions,
            series);
    }

    private static IReadOnlyList<GovForecastProjection> ProjectTrend(
        List<(DateTime T, double V)> obs, decimal baseline, DateTime asOf, int horizon)
    {
        var t0 = obs[0].T;
        var xs = obs.Select(o => (o.T - t0).TotalDays / 30.0).ToArray();
        var ys = obs.Select(o => o.V).ToArray();
        var n = xs.Length;

        var meanX = xs.Average();
        var meanY = ys.Average();
        var sxx = xs.Sum(x => (x - meanX) * (x - meanX));
        var sxy = xs.Zip(ys, (x, y) => (x - meanX) * (y - meanY)).Sum();
        var slope = sxx == 0 ? 0 : sxy / sxx;
        var intercept = meanY - slope * meanX;

        var residuals = xs.Zip(ys, (x, y) => y - (intercept + slope * x)).ToArray();
        var residualStd = Math.Sqrt(residuals.Sum(r => r * r) / Math.Max(1, n - 2));
        var totalSs = ys.Sum(y => (y - meanY) * (y - meanY));
        var r2 = totalSs == 0 ? 1 : 1 - residuals.Sum(r => r * r) / totalSs;

        var confidence = (n, r2) switch
        {
            ( >= 6, >= 0.75) => GovForecastConfidence.High,
            ( >= 3, >= 0.4) => GovForecastConfidence.Moderate,
            _ => GovForecastConfidence.Low,
        };

        var baseX = (asOf - t0).TotalDays / 30.0;
        var list = new List<GovForecastProjection>(horizon);
        for (var m = 1; m <= horizon; m++)
        {
            var x = baseX + m;
            var predicted = Math.Max(0, intercept + slope * x);
            var band = Math.Max(residualStd, Math.Abs(predicted) * 0.08) * (1 + 0.6 * m / horizon);
            list.Add(new GovForecastProjection(
                m,
                asOf.AddMonths(m),
                Round(predicted),
                Round(Math.Max(0, predicted - band)),
                Round(predicted + band),
                confidence));
        }

        return list;
    }

    private static IReadOnlyList<GovForecastProjection> ProjectFlat(
        decimal baseline, DateTime asOf, int horizon, int points)
    {
        var value = (double)baseline;
        var list = new List<GovForecastProjection>(horizon);
        for (var m = 1; m <= horizon; m++)
        {
            var band = Math.Abs(value) * (0.15 + 0.5 * m / horizon);
            list.Add(new GovForecastProjection(
                m,
                asOf.AddMonths(m),
                Round(value),
                Round(Math.Max(0, value - band)),
                Round(value + band),
                GovForecastConfidence.Low));
        }

        return list;
    }

    private static decimal Round(double v) => Math.Round((decimal)v, 2);
}
