namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Lifecycle of a <see cref="Shipment"/> as it moves through the delivery network.</summary>
public enum ShipmentStatus
{
    /// <summary>Record created; nothing physical yet.</summary>
    Created,
    PartnerAssigned,
    PickupRequested,

    /// <summary>Shipping label / manifest produced, awaiting collection.</summary>
    LabelCreated,

    PickedUp,
    PickupFailed,
    InTransit,
    AtHub,
    OutForDelivery,
    Delivered,

    /// <summary>A delivery attempt failed; may be retried.</summary>
    DeliveryFailed,
    Rescheduled,

    /// <summary>Undeliverable and sent back to origin.</summary>
    Returned,

    Cancelled,
}
