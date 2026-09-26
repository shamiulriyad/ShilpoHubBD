using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ICraftHeritageRepository
{
    Task<List<CraftHeritageEntry>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<CraftHeritageEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> SlugExistsAsync(string slug, Guid? exceptId, CancellationToken cancellationToken);
    Task AddAsync(CraftHeritageEntry entry, CancellationToken cancellationToken);
    void Remove(CraftHeritageEntry entry);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
