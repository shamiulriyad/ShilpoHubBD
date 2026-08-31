namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>How the customer is made whole for a <see cref="ReturnRequest"/>.</summary>
public enum ReturnResolutionType
{
    Refund,
    Replacement,
    Repair,
    StoreCredit,
    NoAction,
}
