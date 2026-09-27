using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IProducerPartnershipAuctionLotRepository
{
    Task<ProducerPartnershipAuctionLot?> GetByIdAsync(Guid lotId, CancellationToken cancellationToken);
    Task<ProducerPartnershipAuctionLot?> GetByIdWithBidsAsync(Guid lotId, CancellationToken cancellationToken);
    Task<List<ProducerPartnershipAuctionLot>> GetForAuctionAsync(Guid auctionId, CancellationToken cancellationToken);
    Task<List<ProducerPartnershipAuctionLot>> GetForAuctionWithBidsAsync(Guid auctionId, CancellationToken cancellationToken);
    Task<bool> ExistsForProducerAsync(Guid auctionId, Guid producerId, CancellationToken cancellationToken);
    Task AddAsync(ProducerPartnershipAuctionLot lot, CancellationToken cancellationToken);
    Task<List<ProducerPartnershipAuctionBid>> GetBidsForLotAsync(Guid lotId, CancellationToken cancellationToken);
    Task<List<ProducerPartnershipAuctionBid>> GetBidsForBusinessPartnerAsync(Guid businessPartnerId, Guid? lotId, Guid? auctionId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Atomically inserts <paramref name="bid"/> and raises the lot's denormalized highest-bid
    /// fields, guarded by a single conditional UPDATE (lot still Open AND bid.Amount still beats the
    /// current highest by at least <paramref name="minimumIncrement"/>, or beats the starting bid if
    /// no bid exists yet). Returns false — without writing anything — if the guard fails, which means
    /// a concurrent bid already raised the price (or the lot closed) between validation and this call.
    /// </summary>
    Task<bool> TryPlaceBidAsync(ProducerPartnershipAuctionBid bid, decimal minimumIncrement, CancellationToken cancellationToken);
}
