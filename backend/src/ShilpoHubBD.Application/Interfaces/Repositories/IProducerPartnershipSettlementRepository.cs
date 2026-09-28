using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IProducerPartnershipSettlementRepository
{
    Task<ProducerPartnershipSettlement?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<ProducerPartnershipSettlement>> GetForAgreementAsync(Guid agreementId, CancellationToken cancellationToken);
    Task<(List<ProducerPartnershipSettlement> Items, int TotalCount)> GetPagedAsync(
        ProducerPartnershipSettlementQueryParameters parameters, CancellationToken cancellationToken);

    /// <summary>True if a settlement other than a Rejected one already covers any part of this period for this agreement — the duplicate-settlement / double-counting guard.</summary>
    Task<bool> ExistsNonRejectedOverlappingAsync(Guid agreementId, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken);

    Task AddAsync(ProducerPartnershipSettlement settlement, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Sum of per-unit refund amounts (ReturnItem.UnitRefundAmount * Quantity) for this producer's
    /// products, across return requests refunded within the period. Whole-order refunds recorded
    /// only via Order.RefundAmount with no matching ReturnRequest are not attributable to a specific
    /// producer and are intentionally not included.
    /// </summary>
    Task<decimal> GetRefundDeductionsAsync(Guid producerId, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken);
}
