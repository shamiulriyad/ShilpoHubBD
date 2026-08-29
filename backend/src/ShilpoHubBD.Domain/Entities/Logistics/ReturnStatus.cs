namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Lifecycle of a <see cref="ReturnRequest"/> from request to closure.</summary>
public enum ReturnStatus
{
    Requested,
    Approved,
    Rejected,
    PickupScheduled,
    InTransit,
    Received,
    UnderInspection,
    Inspected,
    Restocked,
    RefundPending,
    Refunded,
    Closed,
    Cancelled,
}
