namespace ShilpoHubBD.Domain.Entities.ProducerPartnership;

public enum ProducerPartnershipSettlementStatus
{
    /// <summary>Freshly calculated; not yet reviewed.</summary>
    Draft,

    /// <summary>Submitted for admin review.</summary>
    PendingApproval,

    /// <summary>Admin has confirmed the calculation is correct and final. This does NOT mean money
    /// was transferred — there is no payout integration; see <see cref="ProducerPartnershipSettlement.PayoutReference"/>.</summary>
    Approved,

    /// <summary>Admin disputes the calculation; it can be recalculated for the same period.</summary>
    Rejected,
}
