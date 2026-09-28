using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Data.Repositories;

public class ProducerPartnershipAuctionRepository : IProducerPartnershipAuctionRepository
{
    private readonly ShilpoHubDbContext _context;

    public ProducerPartnershipAuctionRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<ProducerPartnershipAuction> WithDetails()
        => _context.ProducerPartnershipAuctions
            .Include(a => a.ManagedBy)
            .Include(a => a.Agreements)
            .Include(a => a.Lots)
            .Include(a => a.Participants)
            .AsSplitQuery();

    public Task<ProducerPartnershipAuction?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<(List<ProducerPartnershipAuction> Items, int TotalCount)> GetPagedAsync(
        ProducerPartnershipAuctionQueryParameters parameters, CancellationToken cancellationToken)
    {
        var query = _context.ProducerPartnershipAuctions
            .Include(a => a.Agreements)
            .Include(a => a.Lots)
            .AsSplitQuery()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Status)
            && Enum.TryParse<ProducerPartnershipAuctionStatus>(parameters.Status, true, out var status))
        {
            query = query.Where(a => a.Status == status);
        }

        if (parameters.AuctionYear.HasValue)
        {
            query = query.Where(a => a.AuctionYear == parameters.AuctionYear.Value);
        }

        query = query.OrderByDescending(a => a.AuctionYear).ThenByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken)
        => _context.ProducerPartnershipAuctions.AnyAsync(a => a.Slug == slug, cancellationToken);

    public async Task AddAsync(ProducerPartnershipAuction auction, CancellationToken cancellationToken)
        => await _context.ProducerPartnershipAuctions.AddAsync(auction, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
