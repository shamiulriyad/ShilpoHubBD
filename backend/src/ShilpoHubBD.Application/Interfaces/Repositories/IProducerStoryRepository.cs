using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IProducerStoryRepository
{
    Task<ProducerStory?> GetByProducerIdAsync(Guid producerId, CancellationToken cancellationToken);
    Task<bool> ExistsByProducerIdAsync(Guid producerId, CancellationToken cancellationToken);
    Task<bool> ExistsByHeritageIdAsync(string heritageId, CancellationToken cancellationToken);
    Task AddAsync(ProducerStory story, CancellationToken cancellationToken);
    void Remove(ProducerStory story);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
