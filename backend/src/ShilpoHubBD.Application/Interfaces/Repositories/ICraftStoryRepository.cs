using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ICraftStoryRepository
{
    Task<CraftStory?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CraftStory?> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken);
    Task<bool> ExistsByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken);
    Task AddAsync(CraftStory story, CancellationToken cancellationToken);
    void Remove(CraftStory story);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
