using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IHeritageRouteRepository
{
    Task<(List<HeritageRoute> Items, int TotalCount)> GetPagedAsync(HeritageRouteQueryParameters query, CancellationToken cancellationToken);
    Task<List<HeritageRoute>> GetRecommendedAsync(CancellationToken cancellationToken);
    Task<HeritageRoute?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(HeritageRoute route, CancellationToken cancellationToken);
    void Remove(HeritageRoute route);
    Task AddStopAsync(RouteStop stop, CancellationToken cancellationToken);
    void RemoveStop(RouteStop stop);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
