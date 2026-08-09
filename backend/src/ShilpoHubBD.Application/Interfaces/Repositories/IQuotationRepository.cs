using ShilpoHubBD.Application.DTOs.Quotations;
using ShilpoHubBD.Domain.Entities.Quotations;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IQuotationRepository
{
    Task<QuotationRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken);
    Task<QuotationRequestProducer?> GetRecipientAsync(Guid quotationRequestId, Guid producerId, CancellationToken cancellationToken);
    Task<QuotationResponse?> GetResponseByIdAsync(Guid quotationRequestId, Guid responseId, CancellationToken cancellationToken);
    Task<QuotationResponse?> GetResponseByIdAsync(Guid responseId, CancellationToken cancellationToken);
    Task<(List<QuotationRequest> Items, int TotalCount)> GetPagedForBusinessPartnerAsync(Guid businessPartnerId, QuotationQueryParameters parameters, CancellationToken cancellationToken);
    Task<(List<QuotationRequest> Items, int TotalCount)> GetPagedForProducerAsync(Guid producerId, QuotationQueryParameters parameters, CancellationToken cancellationToken);
    Task<(List<QuotationRequest> Items, int TotalCount)> GetPagedAllAsync(QuotationQueryParameters parameters, CancellationToken cancellationToken);
    Task<(int TotalRecipients, int RespondedCount)> GetRecipientProgressAsync(Guid quotationRequestId, CancellationToken cancellationToken);
    Task<bool> ExistsByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken);
    Task AddAsync(QuotationRequest quotationRequest, CancellationToken cancellationToken);
    Task AddStatusEventAsync(QuotationStatusEvent statusEvent, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
