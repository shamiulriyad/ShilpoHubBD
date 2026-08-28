using ShilpoHubBD.Application.DTOs.ArVr;
using ShilpoHubBD.Application.DTOs.Common;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ICulturalStoryService
{
    Task<PagedResult<CulturalStoryDto>> GetPagedAsync(CulturalStoryQueryParameters query, CancellationToken cancellationToken);
    Task<CulturalStoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CulturalStoryDto> CreateAsync(CreateCulturalStoryRequest request, CancellationToken cancellationToken);
    Task<CulturalStoryDto> UpdateAsync(Guid id, UpdateCulturalStoryRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
