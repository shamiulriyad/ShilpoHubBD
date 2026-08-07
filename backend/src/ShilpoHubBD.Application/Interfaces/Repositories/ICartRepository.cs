using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ICartRepository
{
    Task<List<CartItem>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<CartItem?> GetAsync(Guid userId, Guid productId, Guid? productVariantId, CancellationToken cancellationToken);
    Task<CartItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(CartItem item, CancellationToken cancellationToken);
    void Remove(CartItem item);
    Task ClearAsync(Guid userId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
