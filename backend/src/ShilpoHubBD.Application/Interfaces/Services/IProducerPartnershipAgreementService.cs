using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>
/// The auction-winner → active-partnership workflow: Pending (admin sets terms) →
/// AwaitingProducerConfirmation → AwaitingBPConfirmation → Active (only once both parties have
/// confirmed, terms are complete, and the producer holds no other active partnership) → eventually
/// Suspended/Expired/Cancelled/Completed.
/// </summary>
public interface IProducerPartnershipAgreementService
{
    /// <summary>Admin-only manual creation, for a partnership that didn't come from an auction win.</summary>
    Task<ProducerPartnershipAgreementDto> CreateAsync(Guid createdByUserId, CreateProducerPartnershipAgreementRequest request, CancellationToken cancellationToken);

    /// <summary>Called by the auction service for each lot that ends Awarded — always starts Pending. A no-op if this lot already has an agreement.</summary>
    Task CreateFromAuctionWinAsync(
        Guid auctionId, Guid auctionLotId, Guid producerId, Guid businessPartnerId, decimal winningBidAmount,
        int defaultPartnershipDurationMonths, decimal? defaultProducerSharePercentage, CancellationToken cancellationToken);

    Task<PagedResult<ProducerPartnershipAgreementListItemDto>> GetForBusinessPartnerAsync(Guid businessPartnerId, bool isAdmin, ProducerPartnershipAgreementQueryParameters parameters, CancellationToken cancellationToken);
    Task<PagedResult<ProducerPartnershipAgreementListItemDto>> GetForProducerAsync(Guid producerId, ProducerPartnershipAgreementQueryParameters parameters, CancellationToken cancellationToken);
    Task<ProducerPartnershipAgreementDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);

    /// <summary>Admin-only, only while Pending.</summary>
    Task<ProducerPartnershipAgreementDto> UpdateTermsAsync(Guid id, UpdateProducerPartnershipAgreementTermsRequest request, CancellationToken cancellationToken);

    /// <summary>Admin-only: Pending → AwaitingProducerConfirmation. Requires complete, valid (summing to 100%) terms.</summary>
    Task<ProducerPartnershipAgreementDto> SubmitForConfirmationAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// The producer or the Business Partner confirms, whichever the current status is waiting on.
    /// Activating (on the second confirmation) re-checks every condition: terms complete, no other
    /// active partnership already exists for this producer.
    /// </summary>
    Task<ProducerPartnershipAgreementDto> ConfirmAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken);

    Task<ProducerPartnershipAgreementDto> SuspendAsync(Guid id, string? reason, CancellationToken cancellationToken);
    Task<ProducerPartnershipAgreementDto> ResumeAsync(Guid id, CancellationToken cancellationToken);
    Task<ProducerPartnershipAgreementDto> CompleteAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<ProducerPartnershipAgreementDto> CancelAsync(Guid id, Guid currentUserId, bool isAdmin, TerminateProducerPartnershipAgreementRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Settlement-record generation isn't built yet; this is the guard a future settlement feature
    /// will call first. Throws unless the (lazily expiry-checked) agreement is currently Active.
    /// </summary>
    Task EnsureCanRecordSettlementAsync(Guid id, CancellationToken cancellationToken);
}
