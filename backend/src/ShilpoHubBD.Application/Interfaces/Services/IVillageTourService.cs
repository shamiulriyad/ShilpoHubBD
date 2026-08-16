using ShilpoHubBD.Application.DTOs.ArVr;
using ShilpoHubBD.Application.DTOs.Common;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IVillageTourService
{
    Task<PagedResult<VillageTourStopDto>> GetPagedAsync(VillageTourStopQueryParameters query, CancellationToken cancellationToken);
    Task<VillageTourStopDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<VillageTourStopDto> CreateAsync(CreateVillageTourStopRequest request, CancellationToken cancellationToken);
    Task<VillageTourStopDto> UpdateAsync(Guid id, UpdateVillageTourStopRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
