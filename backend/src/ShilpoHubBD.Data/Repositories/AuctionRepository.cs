using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Auction;

namespace ShilpoHubBD.Data.Repositories;

public class AuctionRepository : IAuctionRepository
{
    private readonly ShilpoHubDbContext _context;

    public AuctionRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<Auction> WithDetails()
        => _context.Auctions
            .Include(a => a.Producer)
            .Include(a => a.Product).ThenInclude(p => p.Images)
            .Include(a => a.Winner)
            .Include(a => a.Bids).ThenInclude(b => b.Bidder)
            .AsSplitQuery();

    public async Task<(List<Auction> Items, int TotalCount)> GetPagedAsync(AuctionStatus? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        var auctions = WithDetails().AsQueryable();

        if (status is not null)
        {
            auctions = auctions.Where(a => a.Status == status);
        }

        auctions = auctions.OrderBy(a => a.EndAt);

        var totalCount = await auctions.CountAsync(cancellationToken);
        var items = await auctions
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task AddAsync(Auction auction, CancellationToken cancellationToken)
        => await _context.Auctions.AddAsync(auction, cancellationToken);

    public async Task AddBidAsync(AuctionBid bid, CancellationToken cancellationToken)
        => await _context.AuctionBids.AddAsync(bid, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
