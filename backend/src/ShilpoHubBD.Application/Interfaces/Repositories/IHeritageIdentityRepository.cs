using ShilpoHubBD.Domain.Entities.HeritageIdentity;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IHeritageIdentityRepository
{
    Task<ProducerHeritageIdentity?> GetByProducerIdAsync(Guid producerId, CancellationToken cancellationToken);
    Task<bool> ExistsByProducerIdAsync(Guid producerId, CancellationToken cancellationToken);
    Task<bool> ExistsByHeritageIdNumberAsync(string heritageIdNumber, CancellationToken cancellationToken);
    Task<(List<ProducerHeritageIdentity> Items, int TotalCount)> GetPagedVerifiedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<(List<ScoreHistoryEntry> Items, int TotalCount)> GetScoreHistoryAsync(
        Guid producerHeritageIdentityId, int page, int pageSize, CancellationToken cancellationToken);
    Task AddAsync(ProducerHeritageIdentity identity, CancellationToken cancellationToken);
    Task AddScoreHistoryEntryAsync(ScoreHistoryEntry entry, CancellationToken cancellationToken);
    void Remove(ProducerHeritageIdentity identity);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
