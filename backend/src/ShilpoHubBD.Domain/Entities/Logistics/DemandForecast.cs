using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>
/// A rule-based forecast of future logistics demand (shipments / pickups / returns / weight) for a
/// partner, optionally scoped to one district or warehouse. Produced by the pluggable
/// demand-forecast provider — OLS trend plus day-of-week seasonality. No real model.
/// </summary>
public class DemandForecast
{
    public Guid Id { get; set; }

    public Guid LogisticsPartnerProfileId { get; set; }
    public LogisticsPartnerProfile Profile { get; set; } = null!;

    public Guid GeneratedByUserId { get; set; }
    public User GeneratedBy { get; set; } = null!;

    public DemandForecastScope Scope { get; set; }
    public Guid? ScopeId { get; set; }
    public string ScopeLabel { get; set; } = string.Empty;

    /// <summary>What is being counted: <c>shipments</c>, <c>pickups</c>, <c>returns</c> or <c>weight_kg</c>.</summary>
    public string Metric { get; set; } = "shipments";

    public int HorizonDays { get; set; }
    public string Granularity { get; set; } = "day";

    public string Method { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public AiLogisticsConfidence Confidence { get; set; }

    public double BaselineDailyAverage { get; set; }
    public double PredictedTotal { get; set; }

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    public string? AssumptionsJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<DemandForecastPoint> Points { get; set; } = new List<DemandForecastPoint>();
}
