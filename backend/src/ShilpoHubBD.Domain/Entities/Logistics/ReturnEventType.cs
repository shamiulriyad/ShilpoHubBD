namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Kind of entry on a <see cref="ReturnEvent"/> audit trail.</summary>
public enum ReturnEventType
{
    Created,
    Approved,
    Rejected,
    PickupScheduled,
    StatusChanged,
    InspectionCompleted,
    Restocked,
    RefundInitiated,
    RefundCompleted,
    Closed,
    Cancelled,
    NoteAdded,
}
