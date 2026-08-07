namespace ShilpoHubBD.Domain.Entities.Commerce;

public enum PaymentStatus
{
    Pending,
    Awaiting,
    Paid,
    Failed,
    Refunded,
    PartiallyRefunded,
}
