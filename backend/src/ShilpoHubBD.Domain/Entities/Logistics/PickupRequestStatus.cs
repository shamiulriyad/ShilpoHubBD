namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Lifecycle of a <see cref="PickupRequest"/> from creation to collection.</summary>
public enum PickupRequestStatus
{
    /// <summary>Being prepared; not yet on the schedule.</summary>
    Draft,

    /// <summary>Placed on the pickup schedule with a window but no crew yet.</summary>
    Scheduled,

    /// <summary>A driver / vehicle has been assigned.</summary>
    Assigned,

    /// <summary>Crew is on the way to the origin.</summary>
    EnRoute,

    /// <summary>Goods have been physically collected from the origin.</summary>
    Collected,

    /// <summary>Collected goods handed off to the next leg (warehouse / route).</summary>
    Completed,

    Cancelled,

    Failed,
}
