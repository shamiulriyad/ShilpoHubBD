using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken);
    Task<Dictionary<Guid, int>> GetActiveProductCountsAsync(CancellationToken cancellationToken);
    Task AddAsync(Category category, CancellationToken cancellationToken);
    void Remove(Category category);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
