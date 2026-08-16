using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ICulturalEventService
{
    Task<PagedResult<CulturalEventDto>> GetPagedAsync(CulturalEventQueryParameters query, CancellationToken cancellationToken);
    Task<CulturalEventDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CulturalEventDto> CreateAsync(CreateCulturalEventRequest request, CancellationToken cancellationToken);
    Task<CulturalEventDto> UpdateAsync(Guid id, UpdateCulturalEventRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
