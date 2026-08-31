namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>State of an AI <see cref="RouteOptimizationRun"/> relative to the route it targets.</summary>
public enum RouteOptimizationRunStatus
{
    Proposed,
    Applied,
    Discarded,
}
