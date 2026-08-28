using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.ManufacturingPartnership;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.ManufacturingPartnership;

namespace ShilpoHubBD.Data.Repositories;

public class PartnershipRepository : IPartnershipRepository
{
    private readonly ShilpoHubDbContext _context;

    public PartnershipRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<ManufacturingPartnership> WithDetails()
        => _context.ManufacturingPartnerships
            .Include(p => p.BusinessPartner)
            .Include(p => p.Producer)
            .Include(p => p.Milestones)
            .Include(p => p.StatusHistory)
            .AsSplitQuery();

    private IQueryable<ManufacturingPartnership> ForListing()
        => _context.ManufacturingPartnerships
            .Include(p => p.Producer)
            .Include(p => p.Milestones)
            .AsSplitQuery();

    public Task<ManufacturingPartnership?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<ManufacturingMilestone?> GetMilestoneAsync(Guid partnershipId, Guid milestoneId, CancellationToken cancellationToken)
        => _context.ManufacturingMilestones
            .Include(m => m.Partnership)
            .FirstOrDefaultAsync(m => m.Id == milestoneId && m.PartnershipId == partnershipId, cancellationToken);

    private static async Task<(List<ManufacturingPartnership> Items, int TotalCount)> PageAsync(
        IQueryable<ManufacturingPartnership> query, PartnershipQueryParameters parameters, CancellationToken cancellationToken)
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

    public Task<(List<ManufacturingPartnership> Items, int TotalCount)> GetPagedForBusinessPartnerAsync(
        Guid businessPartnerId, PartnershipQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing().Where(p => p.BusinessPartnerId == businessPartnerId), parameters, cancellationToken);

    public Task<(List<ManufacturingPartnership> Items, int TotalCount)> GetPagedForProducerAsync(
        Guid producerId, PartnershipQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing().Where(p => p.ProducerId == producerId), parameters, cancellationToken);

    public Task<(List<ManufacturingPartnership> Items, int TotalCount)> GetPagedAllAsync(
        PartnershipQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing(), parameters, cancellationToken);

    public Task<bool> ExistsByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken)
        => _context.ManufacturingPartnerships.AnyAsync(p => p.ReferenceNumber == referenceNumber, cancellationToken);

    public async Task AddAsync(ManufacturingPartnership partnership, CancellationToken cancellationToken)
        => await _context.ManufacturingPartnerships.AddAsync(partnership, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
