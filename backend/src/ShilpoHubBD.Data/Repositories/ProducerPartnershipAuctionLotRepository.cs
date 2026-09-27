using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Data.Repositories;

public class ProducerPartnershipAuctionLotRepository : IProducerPartnershipAuctionLotRepository
{
    private readonly ShilpoHubDbContext _context;

    public ProducerPartnershipAuctionLotRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<ProducerPartnershipAuctionLot?> GetByIdAsync(Guid lotId, CancellationToken cancellationToken)
        => _context.ProducerPartnershipAuctionLots
            .Include(l => l.Producer)
            .Include(l => l.CurrentHighestBidder)
            .Include(l => l.WinningBid!).ThenInclude(b => b.BusinessPartner)
            .Include(l => l.Auction)
            .FirstOrDefaultAsync(l => l.Id == lotId, cancellationToken);

    public Task<ProducerPartnershipAuctionLot?> GetByIdWithBidsAsync(Guid lotId, CancellationToken cancellationToken)
        => _context.ProducerPartnershipAuctionLots
            .Include(l => l.Producer)
            .Include(l => l.Auction)
            .Include(l => l.Bids)
            .FirstOrDefaultAsync(l => l.Id == lotId, cancellationToken);

    public Task<List<ProducerPartnershipAuctionLot>> GetForAuctionAsync(Guid auctionId, CancellationToken cancellationToken)
        => _context.ProducerPartnershipAuctionLots
            .Include(l => l.Producer)
            .Include(l => l.CurrentHighestBidder)
            .Include(l => l.WinningBid!).ThenInclude(b => b.BusinessPartner)
            .Include(l => l.Auction)
            .Where(l => l.AuctionId == auctionId)
            .OrderBy(l => l.CreatedAt)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

    public Task<List<ProducerPartnershipAuctionLot>> GetForAuctionWithBidsAsync(Guid auctionId, CancellationToken cancellationToken)
        => _context.ProducerPartnershipAuctionLots
            .Include(l => l.Bids)
            .Where(l => l.AuctionId == auctionId)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsForProducerAsync(Guid auctionId, Guid producerId, CancellationToken cancellationToken)
        => _context.ProducerPartnershipAuctionLots.AnyAsync(l => l.AuctionId == auctionId && l.ProducerId == producerId, cancellationToken);

    public async Task AddAsync(ProducerPartnershipAuctionLot lot, CancellationToken cancellationToken)
        => await _context.ProducerPartnershipAuctionLots.AddAsync(lot, cancellationToken);

    public Task<List<ProducerPartnershipAuctionBid>> GetBidsForLotAsync(Guid lotId, CancellationToken cancellationToken)
        => _context.ProducerPartnershipAuctionBids
            .Include(b => b.BusinessPartner)
            .Where(b => b.LotId == lotId)
            .OrderByDescending(b => b.Amount)
            .ThenBy(b => b.PlacedAt)
            .ToListAsync(cancellationToken);

    public Task<List<ProducerPartnershipAuctionBid>> GetBidsForBusinessPartnerAsync(
        Guid businessPartnerId, Guid? lotId, Guid? auctionId, CancellationToken cancellationToken)
    {
        var query = _context.ProducerPartnershipAuctionBids
            .Include(b => b.Lot).ThenInclude(l => l.Producer)
            .Where(b => b.BusinessPartnerId == businessPartnerId);

        if (lotId.HasValue)
        {
            query = query.Where(b => b.LotId == lotId.Value);
        }

        if (auctionId.HasValue)
        {
            query = query.Where(b => b.Lot.AuctionId == auctionId.Value);
        }

        return query.OrderByDescending(b => b.PlacedAt).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);

    public async Task<bool> TryPlaceBidAsync(ProducerPartnershipAuctionBid bid, decimal minimumIncrement, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var amount = bid.Amount;
        var rowsAffected = await _context.ProducerPartnershipAuctionLots
            .Where(l => l.Id == bid.LotId
                && l.Status == ProducerPartnershipAuctionLotStatus.Open
                && ((l.CurrentHighestBid == null && amount >= l.StartingBid)
                    || (l.CurrentHighestBid != null && amount >= l.CurrentHighestBid.Value + minimumIncrement)))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(l => l.CurrentHighestBid, amount)
                .SetProperty(l => l.CurrentHighestBidderId, bid.BusinessPartnerId)
                .SetProperty(l => l.BidCount, l => l.BidCount + 1)
                .SetProperty(l => l.UpdatedAt, bid.PlacedAt), cancellationToken);

        if (rowsAffected == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        await _context.ProducerPartnershipAuctionBids.AddAsync(bid, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }
}
