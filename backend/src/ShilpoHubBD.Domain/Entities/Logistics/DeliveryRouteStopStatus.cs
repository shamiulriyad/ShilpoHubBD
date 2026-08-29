namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Execution state of a single <see cref="DeliveryRouteStop"/>.</summary>
public enum DeliveryRouteStopStatus
{
    Pending,
    EnRoute,
    Arrived,
    Completed,
    Skipped,
    Failed,
}
