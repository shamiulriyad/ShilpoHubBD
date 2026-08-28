using ShilpoHubBD.Application.DTOs.ArVr;
using ShilpoHubBD.Domain.Entities.ArVr;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IMuseumItemRepository
{
    Task<(List<MuseumItem> Items, int TotalCount)> GetPagedAsync(MuseumItemQueryParameters query, CancellationToken cancellationToken);
    Task<MuseumItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(MuseumItem item, CancellationToken cancellationToken);
    void Remove(MuseumItem item);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
