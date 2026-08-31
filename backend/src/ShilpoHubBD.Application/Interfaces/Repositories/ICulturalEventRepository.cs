using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ICulturalEventRepository
{
    Task<(List<CulturalEvent> Items, int TotalCount)> GetPagedAsync(CulturalEventQueryParameters query, CancellationToken cancellationToken);
    Task<CulturalEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(CulturalEvent culturalEvent, CancellationToken cancellationToken);
    void Remove(CulturalEvent culturalEvent);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
