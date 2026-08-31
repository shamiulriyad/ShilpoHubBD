namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>One line of a <see cref="RouteOptimizationRun"/>'s proposed stop order.</summary>
public class RouteOptimizationRunStop
{
    public Guid Id { get; set; }

    public Guid RouteOptimizationRunId { get; set; }
    public RouteOptimizationRun RouteOptimizationRun { get; set; } = null!;

    /// <summary>The <see cref="DeliveryRouteStop"/> this line refers to. Not a FK — the route can change.</summary>
    public Guid RouteStopId { get; set; }

    public int OriginalSequence { get; set; }
    public int ProposedSequence { get; set; }
    public decimal? DistanceFromPreviousKm { get; set; }
    public string Label { get; set; } = string.Empty;
}
