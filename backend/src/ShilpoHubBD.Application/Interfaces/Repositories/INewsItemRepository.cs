using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface INewsItemRepository
{
    Task<(List<NewsItem> Items, int TotalCount)> GetPagedAsync(NewsItemQueryParameters query, bool publishedOnly, CancellationToken cancellationToken);
    Task<(List<NewsItem> Items, int TotalCount)> GetDraftsAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<NewsItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<NewsItem?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken);
    Task AddAsync(NewsItem item, CancellationToken cancellationToken);
    void Remove(NewsItem item);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
