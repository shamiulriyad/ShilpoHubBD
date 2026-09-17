using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Application.DTOs.Common;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IBlogPostService
{
    Task<PagedResult<BlogPostListItemDto>> GetPagedAsync(BlogPostQueryParameters query, CancellationToken cancellationToken);
    Task<PagedResult<BlogPostListItemDto>> GetDraftsAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<BlogPostDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<BlogPostDto> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<BlogPostDto> CreateAsync(Guid authorUserId, CreateBlogPostRequest request, CancellationToken cancellationToken);
    Task<BlogPostDto> UpdateAsync(Guid id, UpdateBlogPostRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
