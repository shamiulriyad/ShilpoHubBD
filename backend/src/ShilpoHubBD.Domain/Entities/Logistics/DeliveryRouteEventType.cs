namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Kind of entry recorded on a <see cref="DeliveryRouteEvent"/> audit trail.</summary>
public enum DeliveryRouteEventType
{
    Created,
    Updated,
    StopAdded,
    StopRemoved,
    Resequenced,
    Optimized,
    Assigned,
    Dispatched,
    Started,
    StopArrived,
    StopCompleted,
    StopSkipped,
    StopFailed,
    Completed,
    Cancelled,
    NoteAdded,
}
