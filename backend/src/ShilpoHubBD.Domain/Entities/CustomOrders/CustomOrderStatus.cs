namespace ShilpoHubBD.Domain.Entities.CustomOrders;

public enum CustomOrderStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
    InProgress = 3,
    Completed = 4,
    Cancelled = 5,

    /// <summary>Handed to a logistics partner; the partner, not the producer, delivers it.</summary>
    Shipped = 6,

    /// <summary>The logistics partner confirmed delivery.</summary>
    Delivered = 7,
}
