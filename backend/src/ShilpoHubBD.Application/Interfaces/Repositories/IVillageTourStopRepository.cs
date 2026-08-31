using ShilpoHubBD.Application.DTOs.ArVr;
using ShilpoHubBD.Domain.Entities.ArVr;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IVillageTourStopRepository
{
    Task<(List<VillageTourStop> Items, int TotalCount)> GetPagedAsync(VillageTourStopQueryParameters query, CancellationToken cancellationToken);
    Task<VillageTourStop?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(VillageTourStop stop, CancellationToken cancellationToken);
    void Remove(VillageTourStop stop);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
