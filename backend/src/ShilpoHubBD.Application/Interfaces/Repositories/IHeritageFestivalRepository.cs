using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IHeritageFestivalRepository
{
    Task<(List<HeritageFestival> Items, int TotalCount)> GetPagedAsync(HeritageFestivalQueryParameters query, CancellationToken cancellationToken);
    Task<HeritageFestival?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(HeritageFestival festival, CancellationToken cancellationToken);
    void Remove(HeritageFestival festival);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
