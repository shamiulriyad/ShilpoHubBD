namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>Result of a single <see cref="DeliveryAttempt"/>.</summary>
public enum DeliveryAttemptOutcome
{
    Delivered,
    RecipientUnavailable,
    AddressNotFound,
    Refused,
    Rescheduled,
    Damaged,
    Other,
}
