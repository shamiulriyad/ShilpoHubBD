using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>
/// Projects future logistics demand from a daily history series. The default implementation fits an
/// OLS trend and overlays day-of-week seasonality; swap for a real time-series model later without
/// touching the service or controller.
/// </summary>
public interface IDemandForecastProvider
{
    string ProviderName { get; }

    LogisticsDemandForecastResult Forecast(LogisticsDemandForecastInput input);
}

public record LogisticsDemandForecastInput
{
    public string Metric { get; init; } = "shipments";
    public int HorizonDays { get; init; } = 14;
    public string Granularity { get; init; } = "day";
    public DateTime AsOf { get; init; }

    /// <summary>Daily observations, oldest first, one per calendar day in the lookback window.</summary>
    public IReadOnlyList<LogisticsDemandObservation> History { get; init; } = Array.Empty<LogisticsDemandObservation>();
}

public record LogisticsDemandObservation(DateTime Date, double Value);

public record LogisticsDemandForecastResult(
    string Method,
    string Summary,
    string AssumptionsJson,
    double BaselineDailyAverage,
    double PredictedTotal,
    AiLogisticsConfidence Confidence,
    IReadOnlyList<LogisticsDemandForecastPointResult> Points);

public record LogisticsDemandForecastPointResult(
    DateTime PeriodDate,
    double PredictedValue,
    double LowerBound,
    double UpperBound);
