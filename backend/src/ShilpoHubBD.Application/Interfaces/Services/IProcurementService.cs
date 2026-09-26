using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Procurement;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IProcurementService
{
    Task<ProcurementRequestDto> CreateAsync(Guid businessPartnerId, CreateProcurementRequest request, CancellationToken cancellationToken);
    Task<ProcurementRequestDto> CreateFromQuotationResponseAsync(Guid businessPartnerId, bool isAdmin, Guid quotationResponseId, CreateProcurementFromQuotationRequest request, CancellationToken cancellationToken);

    Task<PagedResult<ProcurementRequestListItemDto>> GetForBusinessPartnerAsync(Guid businessPartnerId, bool isAdmin, ProcurementQueryParameters parameters, CancellationToken cancellationToken);
    Task<ProcurementRequestDto> GetByIdAsync(Guid id, Guid businessPartnerId, bool isAdmin, CancellationToken cancellationToken);

    // The selected producer (or an admin) accepts or declines the request.
    Task<ProcurementRequestDto> ApproveAsync(Guid id, Guid userId, bool isAdmin, ProcurementDecisionRequest request, CancellationToken cancellationToken);
    Task<ProcurementRequestDto> RejectAsync(Guid id, Guid userId, bool isAdmin, ProcurementDecisionRequest request, CancellationToken cancellationToken);
    Task<PagedResult<ProcurementRequestListItemDto>> GetForProducerAsync(Guid producerId, ProcurementQueryParameters parameters, CancellationToken cancellationToken);

    // The business partner pays at least 50% up front; an admin then inspects the deal.
    Task<ProcurementRequestDto> PayAdvanceAsync(Guid id, Guid businessPartnerId, bool isAdmin, PayProcurementAdvanceRequest request, CancellationToken cancellationToken);
    Task<PagedResult<ProcurementRequestListItemDto>> GetForInspectionAsync(bool pendingOnly, ProcurementQueryParameters parameters, CancellationToken cancellationToken);
    Task<ProcurementRequestDto> InspectAsync(Guid id, Guid adminUserId, ProcurementInspectionRequest request, CancellationToken cancellationToken);
    Task<ProcurementRequestDto> ConvertToOrderAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken);
    Task<ProcurementRequestDto> CancelAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken);
}
