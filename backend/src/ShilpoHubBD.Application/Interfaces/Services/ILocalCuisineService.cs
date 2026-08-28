using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ILocalCuisineService
{
    Task<PagedResult<LocalCuisineDto>> GetPagedAsync(LocalCuisineQueryParameters query, CancellationToken cancellationToken);
    Task<LocalCuisineDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<LocalCuisineDto> CreateAsync(CreateLocalCuisineRequest request, CancellationToken cancellationToken);
    Task<LocalCuisineDto> UpdateAsync(Guid id, UpdateLocalCuisineRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
