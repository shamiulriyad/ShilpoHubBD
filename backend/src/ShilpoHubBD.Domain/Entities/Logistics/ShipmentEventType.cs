namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Kind of entry on a <see cref="ShipmentTrackingEvent"/> timeline.</summary>
public enum ShipmentEventType
{
    Created,
    StatusChanged,
    LocationUpdated,
    PickedUp,
    ArrivedAtHub,
    DepartedHub,
    OutForDelivery,
    DeliveryAttempted,
    Delivered,
    Exception,
    Returned,
    Cancelled,
    NoteAdded,
}
