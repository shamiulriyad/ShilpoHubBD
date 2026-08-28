using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>
/// Turns policy inputs + a live baseline into projected outcomes and recommendations. The default
/// implementation is rule-based; the abstraction leaves room for a Gemini / OpenAI / custom ML
/// backend later without touching the service or controller.
/// </summary>
public interface IPolicySimulationProvider
{
    string ProviderName { get; }

    PolicySimulationResult Simulate(PolicySimulationInput input);
}

public record PolicySimulationInput
{
    public PolicySimulationType SimulationType { get; init; }
    public HeritageIndexScope Scope { get; init; }
    public string ScopeLabel { get; init; } = "National";
    public int HorizonMonths { get; init; } = 12;

    // User-supplied knobs (all optional; the provider substitutes sensible defaults).
    public decimal? Budget { get; init; }
    public int? TargetBeneficiaries { get; init; }
    public int? DurationMonths { get; init; }
    public double? IntensityPercent { get; init; }
    public string? FocusCraft { get; init; }

    public PolicyBaselineSignals Baseline { get; init; } = new();
}

/// <summary>Current-state numbers the projection is measured against.</summary>
public record PolicyBaselineSignals
{
    public int Producers { get; init; }
    public int ActiveProducers { get; init; }
    public int Employment { get; init; }
    public int ApprenticesInPipeline { get; init; }
    public decimal ExportValue { get; init; }
    public decimal TourismRevenue { get; init; }
    public decimal EconomyValue { get; init; }
    public decimal MarketplaceSalesValue { get; init; }
    public decimal AverageOrderValue { get; init; }
    public int TourismBookings { get; init; }
}

public record PolicySimulationResult(
    string Summary,
    string Method,
    PolicySimulationConfidence Confidence,
    string AssumptionsJson,
    IReadOnlyList<PolicyProjectionResult> Projections,
    IReadOnlyList<PolicyRecommendationResult> Recommendations);

public record PolicyProjectionResult(
    string Metric,
    string Unit,
    decimal BaselineValue,
    decimal ProjectedValue,
    PolicySimulationConfidence Confidence,
    string? Detail);

public record PolicyRecommendationResult(
    PolicyRecommendationPriority Priority,
    string Title,
    string Detail);
