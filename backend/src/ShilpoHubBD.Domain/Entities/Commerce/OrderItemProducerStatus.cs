namespace ShilpoHubBD.Domain.Entities.Commerce;

public enum OrderItemProducerStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
    Processing = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6,
}
