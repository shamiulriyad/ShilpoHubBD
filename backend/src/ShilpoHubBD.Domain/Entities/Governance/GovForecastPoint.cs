namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>One projected value in a <see cref="GovForecast"/> — a metric at a future month.</summary>
public class GovForecastPoint
{
    public Guid Id { get; set; }

    public Guid GovForecastId { get; set; }
    public GovForecast Forecast { get; set; } = null!;

    public string Metric { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;

    /// <summary>Months ahead of the baseline (1..HorizonMonths).</summary>
    public int MonthOffset { get; set; }
    public DateTime PeriodDate { get; set; }

    public decimal BaselineValue { get; set; }
    public decimal ProjectedValue { get; set; }
    public decimal LowerBound { get; set; }
    public decimal UpperBound { get; set; }

    public GovForecastConfidence Confidence { get; set; }
    public int DisplayOrder { get; set; }
}
