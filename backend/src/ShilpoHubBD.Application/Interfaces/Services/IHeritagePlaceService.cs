using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IHeritagePlaceService
{
    Task<PagedResult<HeritagePlaceDto>> GetPagedAsync(HeritagePlaceQueryParameters query, CancellationToken cancellationToken);
    Task<PagedResult<HeritagePlaceDto>> GetNearbyAsync(NearbyHeritagePlaceQueryParameters query, CancellationToken cancellationToken);
    Task<HeritagePlaceDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<HeritagePlaceDto> CreateAsync(CreateHeritagePlaceRequest request, CancellationToken cancellationToken);
    Task<HeritagePlaceDto> UpdateAsync(Guid id, UpdateHeritagePlaceRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
