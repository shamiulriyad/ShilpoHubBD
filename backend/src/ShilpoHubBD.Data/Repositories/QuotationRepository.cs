using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Quotations;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Quotations;

namespace ShilpoHubBD.Data.Repositories;

public class QuotationRepository : IQuotationRepository
{
    private readonly ShilpoHubDbContext _context;

    public QuotationRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<QuotationRequest> WithDetails()
        => _context.QuotationRequests
            .Include(q => q.BusinessPartner)
            .Include(q => q.Items).ThenInclude(i => i.Category)
            .Include(q => q.Items).ThenInclude(i => i.Product)
            .Include(q => q.Recipients).ThenInclude(r => r.Producer)
            .Include(q => q.Recipients).ThenInclude(r => r.Response!).ThenInclude(res => res.Items).ThenInclude(ri => ri.QuotationRequestItem)
            .Include(q => q.StatusHistory)
            .AsSplitQuery();

    private IQueryable<QuotationRequest> ForListing()
        => _context.QuotationRequests
            .Include(q => q.Items)
            .Include(q => q.Recipients).ThenInclude(r => r.Response)
            .AsSplitQuery();

    public Task<QuotationRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

    public Task<QuotationRequestProducer?> GetRecipientAsync(Guid quotationRequestId, Guid producerId, CancellationToken cancellationToken)
        => _context.QuotationRequestProducers
            .Include(r => r.Response).ThenInclude(res => res!.Items)
            .Include(r => r.QuotationRequest).ThenInclude(q => q.Items)
            .FirstOrDefaultAsync(r => r.QuotationRequestId == quotationRequestId && r.ProducerId == producerId, cancellationToken);

    public Task<QuotationResponse?> GetResponseByIdAsync(Guid quotationRequestId, Guid responseId, CancellationToken cancellationToken)
        => _context.QuotationResponses
            .Include(r => r.QuotationRequestProducer).ThenInclude(rp => rp.QuotationRequest)
            .Include(r => r.QuotationRequestProducer).ThenInclude(rp => rp.Producer)
            .Include(r => r.Items).ThenInclude(i => i.QuotationRequestItem)
            .FirstOrDefaultAsync(r => r.Id == responseId && r.QuotationRequestProducer.QuotationRequestId == quotationRequestId, cancellationToken);

    public Task<QuotationResponse?> GetResponseByIdAsync(Guid responseId, CancellationToken cancellationToken)
        => _context.QuotationResponses
            .Include(r => r.QuotationRequestProducer).ThenInclude(rp => rp.QuotationRequest)
            .Include(r => r.QuotationRequestProducer).ThenInclude(rp => rp.Producer)
            .Include(r => r.Items).ThenInclude(i => i.QuotationRequestItem)
            .FirstOrDefaultAsync(r => r.Id == responseId, cancellationToken);

    private static async Task<(List<QuotationRequest> Items, int TotalCount)> PageAsync(
        IQueryable<QuotationRequest> query, QuotationQueryParameters parameters, CancellationToken cancellationToken)
    {
        if (parameters.Status.HasValue)
        {
            query = query.Where(q => q.Status == parameters.Status.Value);
        }

        query = query.OrderByDescending(q => q.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<(List<QuotationRequest> Items, int TotalCount)> GetPagedForBusinessPartnerAsync(
        Guid businessPartnerId, QuotationQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing().Where(q => q.BusinessPartnerId == businessPartnerId), parameters, cancellationToken);

    public Task<(List<QuotationRequest> Items, int TotalCount)> GetPagedForProducerAsync(
        Guid producerId, QuotationQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing().Where(q => q.Recipients.Any(r => r.ProducerId == producerId)), parameters, cancellationToken);

    public Task<(List<QuotationRequest> Items, int TotalCount)> GetPagedAllAsync(
        QuotationQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing(), parameters, cancellationToken);

    public async Task<(int TotalRecipients, int RespondedCount)> GetRecipientProgressAsync(Guid quotationRequestId, CancellationToken cancellationToken)
    {
        var total = await _context.QuotationRequestProducers
            .CountAsync(r => r.QuotationRequestId == quotationRequestId, cancellationToken);
        var responded = await _context.QuotationRequestProducers
            .CountAsync(r => r.QuotationRequestId == quotationRequestId && r.Response != null, cancellationToken);

        return (total, responded);
    }

    public Task<bool> ExistsByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken)
        => _context.QuotationRequests.AnyAsync(q => q.ReferenceNumber == referenceNumber, cancellationToken);

    public async Task AddAsync(QuotationRequest quotationRequest, CancellationToken cancellationToken)
        => await _context.QuotationRequests.AddAsync(quotationRequest, cancellationToken);

    public async Task AddStatusEventAsync(QuotationStatusEvent statusEvent, CancellationToken cancellationToken)
        => await _context.QuotationStatusEvents.AddAsync(statusEvent, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
