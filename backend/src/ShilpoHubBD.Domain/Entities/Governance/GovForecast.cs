using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>
/// A forward projection of national heritage-economy metrics ("AI Predictions"). Produced by a
/// replaceable rule-based forecasting provider from dashboard-snapshot history and current figures —
/// no real ML model yet.
/// </summary>
public class GovForecast
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>Provider identity, e.g. "rule-based-gov-forecast-v1".</summary>
    public string Method { get; set; } = string.Empty;

    public int HorizonMonths { get; set; }

    /// <summary>The date the baseline figures were taken as of.</summary>
    public DateTime BaselineAsOf { get; set; }

    public string? AssumptionsJson { get; set; }
    public string Summary { get; set; } = string.Empty;

    public DateTime GeneratedAt { get; set; }
    public Guid GeneratedByUserId { get; set; }
    public User GeneratedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public ICollection<GovForecastPoint> Points { get; set; } = new List<GovForecastPoint>();
}
