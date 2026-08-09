using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Procurement;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Procurement;

namespace ShilpoHubBD.Data.Repositories;

public class ProcurementRepository : IProcurementRepository
{
    private readonly ShilpoHubDbContext _context;

    public ProcurementRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<ProcurementRequest> WithDetails()
        => _context.ProcurementRequests
            .Include(p => p.BusinessPartner)
            .Include(p => p.Producer)
            .Include(p => p.ApprovedBy)
            .Include(p => p.Order)
            .Include(p => p.Items).ThenInclude(i => i.Product)
            .Include(p => p.StatusHistory)
            .AsSplitQuery();

    private IQueryable<ProcurementRequest> ForListing()
        => _context.ProcurementRequests
            .Include(p => p.Producer)
            .Include(p => p.Items)
            .AsSplitQuery();

    public Task<ProcurementRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    private static async Task<(List<ProcurementRequest> Items, int TotalCount)> PageAsync(
        IQueryable<ProcurementRequest> query, ProcurementQueryParameters parameters, CancellationToken cancellationToken)
    {
        if (parameters.Status.HasValue)
        {
            query = query.Where(p => p.Status == parameters.Status.Value);
        }

        query = query.OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<(List<ProcurementRequest> Items, int TotalCount)> GetPagedForBusinessPartnerAsync(
        Guid businessPartnerId, ProcurementQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing().Where(p => p.BusinessPartnerId == businessPartnerId), parameters, cancellationToken);

    public Task<(List<ProcurementRequest> Items, int TotalCount)> GetPagedAllAsync(
        ProcurementQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing(), parameters, cancellationToken);

    public Task<bool> ExistsByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken)
        => _context.ProcurementRequests.AnyAsync(p => p.ReferenceNumber == referenceNumber, cancellationToken);

    public async Task AddAsync(ProcurementRequest procurementRequest, CancellationToken cancellationToken)
        => await _context.ProcurementRequests.AddAsync(procurementRequest, cancellationToken);

    public async Task AddStatusEventAsync(ProcurementStatusEvent statusEvent, CancellationToken cancellationToken)
        => await _context.ProcurementStatusEvents.AddAsync(statusEvent, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
