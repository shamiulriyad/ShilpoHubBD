using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IWishlistRepository
{
    Task<List<WishlistItem>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<WishlistItem?> GetAsync(Guid userId, Guid productId, CancellationToken cancellationToken);
    Task AddAsync(WishlistItem item, CancellationToken cancellationToken);
    void Remove(WishlistItem item);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
