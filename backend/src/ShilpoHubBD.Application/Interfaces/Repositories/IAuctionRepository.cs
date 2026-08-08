using ShilpoHubBD.Domain.Entities.Auction;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IAuctionRepository
{
    Task<(List<Auction> Items, int TotalCount)> GetPagedAsync(AuctionStatus? status, int page, int pageSize, CancellationToken cancellationToken);
    Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Auction auction, CancellationToken cancellationToken);
    Task AddBidAsync(AuctionBid bid, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
