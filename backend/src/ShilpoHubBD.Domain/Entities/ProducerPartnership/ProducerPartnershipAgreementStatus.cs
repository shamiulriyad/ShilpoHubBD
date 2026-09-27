namespace ShilpoHubBD.Domain.Entities.ProducerPartnership;

public enum ProducerPartnershipAgreementStatus
{
    /// <summary>Created (usually from an auction win); admin is still setting/reviewing its terms.</summary>
    Pending,

    /// <summary>Terms submitted; waiting for the producer to confirm.</summary>
    AwaitingProducerConfirmation,

    /// <summary>Producer confirmed; waiting for the Business Partner to confirm.</summary>
    AwaitingBPConfirmation,

    /// <summary>Both parties confirmed and every required condition held — the live partnership.</summary>
    Active,

    /// <summary>Temporarily paused by an admin (e.g. a dispute); can resume to Active.</summary>
    Suspended,

    /// <summary>EndDate has passed without being completed or cancelled.</summary>
    Expired,

    /// <summary>Ended early, at any stage, by either party or an admin.</summary>
    Cancelled,

    /// <summary>Ran its full course and was closed out normally.</summary>
    Completed,
}
