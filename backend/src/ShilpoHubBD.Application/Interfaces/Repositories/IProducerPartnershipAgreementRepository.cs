using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IProducerPartnershipAgreementRepository
{
    Task<ProducerPartnershipAgreement?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<ProducerPartnershipAgreement> Items, int TotalCount)> GetPagedForBusinessPartnerAsync(
        Guid businessPartnerId, ProducerPartnershipAgreementQueryParameters parameters, CancellationToken cancellationToken);
    Task<(List<ProducerPartnershipAgreement> Items, int TotalCount)> GetPagedForProducerAsync(
        Guid producerId, ProducerPartnershipAgreementQueryParameters parameters, CancellationToken cancellationToken);
    Task<(List<ProducerPartnershipAgreement> Items, int TotalCount)> GetPagedAllAsync(
        ProducerPartnershipAgreementQueryParameters parameters, CancellationToken cancellationToken);
    Task AddAsync(ProducerPartnershipAgreement agreement, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// True if the producer has an Active agreement — with the given Business Partner when one is
    /// specified, or with anyone at all when null. Used to stop a producer already under an active
    /// partnership from being re-entered as an auction lot, and to stop a Business Partner from
    /// bidding on a producer they already hold an active partnership with.
    /// </summary>
    Task<bool> HasActiveAgreementAsync(Guid producerId, Guid? businessPartnerId, CancellationToken cancellationToken);

    /// <summary>Idempotency guard: a lot can only ever produce one agreement.</summary>
    Task<bool> ExistsForAuctionLotAsync(Guid auctionLotId, CancellationToken cancellationToken);
}
