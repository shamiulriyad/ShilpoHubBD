using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>
/// Proposes a better stop order for a delivery route. The default implementation seeds with
/// nearest-neighbour and refines with a 2-opt pass over haversine legs; swap for a real solver later
/// without touching the service or controller.
/// </summary>
public interface IAiRouteOptimizationProvider
{
    string ProviderName { get; }

    AiRouteOptimizationResult Optimize(AiRouteOptimizationInput input);
}

public record AiRouteOptimizationInput
{
    public string Objective { get; init; } = "proximity";
    public double AverageSpeedKmh { get; init; } = 25.0;
    public double? StartLatitude { get; init; }
    public double? StartLongitude { get; init; }
    public IReadOnlyList<AiRouteStopInput> Stops { get; init; } = Array.Empty<AiRouteStopInput>();
}

public record AiRouteStopInput(
    Guid StopId,
    int OriginalSequence,
    double? Latitude,
    double? Longitude,
    int ServiceMinutes,
    string Label);

public record AiRouteOptimizationResult(
    string Method,
    string Summary,
    double OriginalDistanceKm,
    double ProposedDistanceKm,
    int ProposedDurationMinutes,
    AiLogisticsConfidence Confidence,
    IReadOnlyList<AiRouteStopResult> OrderedStops);

public record AiRouteStopResult(
    Guid StopId,
    int OriginalSequence,
    int ProposedSequence,
    double? DistanceFromPreviousKm,
    string Label);
