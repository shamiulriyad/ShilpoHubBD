using ShilpoHubBD.Application.DTOs.ArVr;
using ShilpoHubBD.Application.DTOs.Common;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IMuseumItemService
{
    Task<PagedResult<MuseumItemDto>> GetPagedAsync(MuseumItemQueryParameters query, CancellationToken cancellationToken);
    Task<MuseumItemDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<MuseumItemDto> CreateAsync(CreateMuseumItemRequest request, CancellationToken cancellationToken);
    Task<MuseumItemDto> UpdateAsync(Guid id, UpdateMuseumItemRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
