using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ILocalCuisineRepository
{
    Task<(List<LocalCuisine> Items, int TotalCount)> GetPagedAsync(LocalCuisineQueryParameters query, CancellationToken cancellationToken);
    Task<LocalCuisine?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(LocalCuisine cuisine, CancellationToken cancellationToken);
    void Remove(LocalCuisine cuisine);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
