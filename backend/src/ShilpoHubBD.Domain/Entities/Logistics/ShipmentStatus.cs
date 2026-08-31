namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Lifecycle of a <see cref="Shipment"/> as it moves through the delivery network.</summary>
public enum ShipmentStatus
{
    /// <summary>Record created; nothing physical yet.</summary>
    Created,

    /// <summary>Shipping label / manifest produced, awaiting collection.</summary>
    LabelCreated,

    PickedUp,
    InTransit,
    AtHub,
    OutForDelivery,
    Delivered,

    /// <summary>A delivery attempt failed; may be retried.</summary>
    DeliveryFailed,

    /// <summary>Undeliverable and sent back to origin.</summary>
    Returned,

    Cancelled,
}
