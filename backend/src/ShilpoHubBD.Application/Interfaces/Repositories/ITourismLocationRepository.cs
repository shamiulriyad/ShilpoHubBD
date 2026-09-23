using ShilpoHubBD.Application.DTOs.Tourism;
using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ITourismLocationRepository
{
    Task<(List<TourismLocation> Items, int TotalCount)> GetPagedAsync(TourismLocationQueryParameters query, CancellationToken cancellationToken);
    Task<TourismLocation?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<TourismLocation>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
    Task AddAsync(TourismLocation location, CancellationToken cancellationToken);
    void Remove(TourismLocation location);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
