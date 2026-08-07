using ShilpoHubBD.Domain.Entities.Community;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IProducerFollowRepository
{
    Task<List<ProducerFollow>> GetByFollowerAsync(Guid followerId, CancellationToken cancellationToken);
    Task<ProducerFollow?> GetAsync(Guid followerId, Guid producerId, CancellationToken cancellationToken);
    Task AddAsync(ProducerFollow follow, CancellationToken cancellationToken);
    void Remove(ProducerFollow follow);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
