namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>One bucket of a <see cref="DemandForecast"/> horizon.</summary>
public class DemandForecastPoint
{
    public Guid Id { get; set; }

    public Guid DemandForecastId { get; set; }
    public DemandForecast DemandForecast { get; set; } = null!;

    public DateTime PeriodDate { get; set; }
    public double PredictedValue { get; set; }
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
}
