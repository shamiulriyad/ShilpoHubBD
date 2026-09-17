using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ICmsEventRepository
{
    Task<(List<CmsEvent> Items, int TotalCount)> GetPagedAsync(CmsEventQueryParameters query, bool publishedOnly, CancellationToken cancellationToken);
    Task<(List<CmsEvent> Items, int TotalCount)> GetDraftsAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<CmsEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CmsEvent?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken);
    Task AddAsync(CmsEvent cmsEvent, CancellationToken cancellationToken);
    void Remove(CmsEvent cmsEvent);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
