using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Application.DTOs.Common;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface INewsItemService
{
    Task<PagedResult<NewsItemListItemDto>> GetPagedAsync(NewsItemQueryParameters query, CancellationToken cancellationToken);
    Task<PagedResult<NewsItemListItemDto>> GetDraftsAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<NewsItemDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<NewsItemDto> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<NewsItemDto> CreateAsync(CreateNewsItemRequest request, CancellationToken cancellationToken);
    Task<NewsItemDto> UpdateAsync(Guid id, UpdateNewsItemRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
