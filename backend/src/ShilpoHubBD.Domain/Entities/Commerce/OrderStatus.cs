namespace ShilpoHubBD.Domain.Entities.Commerce;

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    ReturnRequested,
    Returned,
    Refunded,
}
