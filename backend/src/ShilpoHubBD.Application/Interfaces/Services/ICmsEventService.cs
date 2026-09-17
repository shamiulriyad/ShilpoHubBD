using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Application.DTOs.Common;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ICmsEventService
{
    Task<PagedResult<CmsEventListItemDto>> GetPagedAsync(CmsEventQueryParameters query, CancellationToken cancellationToken);
    Task<PagedResult<CmsEventListItemDto>> GetDraftsAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<CmsEventDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CmsEventDto> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<CmsEventDto> CreateAsync(CreateCmsEventRequest request, CancellationToken cancellationToken);
    Task<CmsEventDto> UpdateAsync(Guid id, UpdateCmsEventRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
