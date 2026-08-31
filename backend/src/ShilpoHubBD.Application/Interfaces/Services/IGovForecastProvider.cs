using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>
/// Projects national heritage-economy metrics forward from historical snapshots and current figures.
/// The default implementation fits a simple linear trend; the abstraction leaves room for a real
/// time-series / ML model later without touching the service or controller.
/// </summary>
public interface IGovForecastProvider
{
    string ProviderName { get; }

    GovForecastResult Forecast(GovForecastInput input);
}

public record GovForecastInput
{
    public int HorizonMonths { get; init; } = 12;
    public DateTime BaselineAsOf { get; init; }

    /// <summary>Current value per metric key.</summary>
    public IReadOnlyDictionary<string, decimal> CurrentValues { get; init; }
        = new Dictionary<string, decimal>();

    /// <summary>Historical observations, oldest first: (period end, metric key → value).</summary>
    public IReadOnlyList<GovForecastObservation> History { get; init; } = Array.Empty<GovForecastObservation>();
}

public record GovForecastObservation(DateTime PeriodEnd, IReadOnlyDictionary<string, decimal> Values);

public record GovForecastResult(
    string Method,
    string Summary,
    string AssumptionsJson,
    IReadOnlyList<GovForecastSeries> Series);

public record GovForecastSeries(
    string Metric,
    string Unit,
    decimal BaselineValue,
    IReadOnlyList<GovForecastProjection> Projections);

public record GovForecastProjection(
    int MonthOffset,
    DateTime PeriodDate,
    decimal ProjectedValue,
    decimal LowerBound,
    decimal UpperBound,
    GovForecastConfidence Confidence);
