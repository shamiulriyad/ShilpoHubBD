using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Quotations;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IQuotationService
{
    Task<QuotationRequestDto> CreateAsync(Guid businessPartnerId, CreateQuotationRequest request, CancellationToken cancellationToken);

    Task<PagedResult<QuotationRequestListItemDto>> GetForBusinessPartnerAsync(Guid businessPartnerId, bool isAdmin, QuotationQueryParameters parameters, CancellationToken cancellationToken);
    Task<PagedResult<QuotationRequestListItemDto>> GetForProducerAsync(Guid producerId, QuotationQueryParameters parameters, CancellationToken cancellationToken);

    Task<QuotationRequestDto> GetByIdForBusinessPartnerAsync(Guid id, Guid businessPartnerId, bool isAdmin, CancellationToken cancellationToken);
    Task<QuotationRequestDto> GetByIdForProducerAsync(Guid id, Guid producerId, CancellationToken cancellationToken);

    Task<List<QuotationResponseDto>> CompareAsync(Guid id, Guid businessPartnerId, bool isAdmin, CancellationToken cancellationToken);

    Task<QuotationResponseDto> SubmitResponseAsync(Guid quotationRequestId, Guid producerId, SubmitQuotationResponseRequest request, CancellationToken cancellationToken);
    Task<QuotationResponseDto> DecideResponseAsync(Guid quotationRequestId, Guid responseId, Guid businessPartnerId, bool isAdmin, QuotationResponseDecisionRequest request, CancellationToken cancellationToken);

    Task<QuotationRequestDto> CancelAsync(Guid id, Guid businessPartnerId, bool isAdmin, CancellationToken cancellationToken);
}
