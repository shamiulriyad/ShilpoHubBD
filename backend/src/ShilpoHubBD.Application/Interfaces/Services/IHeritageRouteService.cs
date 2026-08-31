using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IHeritageRouteService
{
    Task<PagedResult<HeritageRouteDto>> GetPagedAsync(HeritageRouteQueryParameters query, CancellationToken cancellationToken);
    Task<List<HeritageRouteDto>> GetRecommendedAsync(CancellationToken cancellationToken);
    Task<HeritageRouteDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<HeritageRouteDto> CreateAsync(CreateHeritageRouteRequest request, CancellationToken cancellationToken);
    Task<HeritageRouteDto> UpdateAsync(Guid id, UpdateHeritageRouteRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<HeritageRouteDto> AddStopAsync(Guid routeId, CreateRouteStopRequest request, CancellationToken cancellationToken);
    Task<HeritageRouteDto> RemoveStopAsync(Guid routeId, Guid stopId, CancellationToken cancellationToken);
    Task<HeritageRouteDto> ReorderStopsAsync(Guid routeId, ReorderStopsRequest request, CancellationToken cancellationToken);
}
