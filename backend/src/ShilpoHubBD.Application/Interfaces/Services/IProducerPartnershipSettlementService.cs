using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>
/// Settlement calculation and record-keeping for an active partnership. There is no payout
/// integration in this project — this never claims money was transferred, only that a period's
/// revenue split was calculated and (optionally) approved by an admin.
/// </summary>
public interface IProducerPartnershipSettlementService
{
    /// <summary>
    /// Admin-only. Calculates one period's gross revenue (Delivered order items only), refund
    /// deductions, platform fee and the producer/BP shares, and records it as Draft. Refuses to run
    /// for a period the agreement wasn't Active for, for a future period, or one that overlaps an
    /// existing non-rejected settlement for the same agreement.
    /// </summary>
    Task<ProducerPartnershipSettlementDto> GenerateAsync(Guid agreementId, GenerateProducerPartnershipSettlementRequest request, CancellationToken cancellationToken);

    Task<ProducerPartnershipSettlementDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<List<ProducerPartnershipSettlementDto>> GetForAgreementAsync(Guid agreementId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);

    /// <summary>Admin settlement dashboard: every settlement, optionally filtered by agreement/status.</summary>
    Task<PagedResult<ProducerPartnershipSettlementDto>> GetPagedAsync(ProducerPartnershipSettlementQueryParameters parameters, CancellationToken cancellationToken);

    /// <summary>Admin-only. The platform-revenue figure for the Admin dashboard: totals across every Approved settlement.</summary>
    Task<PlatformRevenueSummaryDto> GetPlatformRevenueSummaryAsync(CancellationToken cancellationToken);

    Task<ProducerPartnershipSettlementDto> SubmitForApprovalAsync(Guid id, CancellationToken cancellationToken);
    Task<ProducerPartnershipSettlementDto> ApproveAsync(Guid id, Guid approvedByUserId, CancellationToken cancellationToken);
    Task<ProducerPartnershipSettlementDto> RejectAsync(Guid id, RejectProducerPartnershipSettlementRequest request, CancellationToken cancellationToken);
}
