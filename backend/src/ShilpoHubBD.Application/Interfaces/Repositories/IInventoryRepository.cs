using ShilpoHubBD.Domain.Entities.Inventory;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IInventoryRepository
{
    Task<List<InventoryTransaction>> GetByProductAsync(Guid productId, CancellationToken cancellationToken);
    Task AddAsync(InventoryTransaction transaction, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
