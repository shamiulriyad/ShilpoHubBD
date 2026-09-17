using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IBlogPostRepository
{
    Task<(List<BlogPost> Items, int TotalCount)> GetPagedAsync(BlogPostQueryParameters query, bool publishedOnly, CancellationToken cancellationToken);
    Task<(List<BlogPost> Items, int TotalCount)> GetDraftsAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<BlogPost?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<BlogPost?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken);
    Task AddAsync(BlogPost post, CancellationToken cancellationToken);
    void Remove(BlogPost post);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
