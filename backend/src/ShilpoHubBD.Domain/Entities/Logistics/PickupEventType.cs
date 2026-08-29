namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Kind of entry recorded on a <see cref="PickupEvent"/> audit trail.</summary>
public enum PickupEventType
{
    Created,
    Scheduled,
    Rescheduled,
    Assigned,
    StatusChanged,
    Cancelled,
    Failed,
    ItemsUpdated,
    NoteAdded,
}
