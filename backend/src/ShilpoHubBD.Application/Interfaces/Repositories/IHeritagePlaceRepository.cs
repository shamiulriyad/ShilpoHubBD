using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IHeritagePlaceRepository
{
    Task<(List<HeritagePlace> Items, int TotalCount)> GetPagedAsync(HeritagePlaceQueryParameters query, CancellationToken cancellationToken);

    Task<List<HeritagePlace>> GetActiveWithinBoundsAsync(
        double minLatitude, double maxLatitude, double minLongitude, double maxLongitude, CancellationToken cancellationToken);

    Task<HeritagePlace?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<HeritagePlace>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
    Task AddAsync(HeritagePlace place, CancellationToken cancellationToken);
    void Remove(HeritagePlace place);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
