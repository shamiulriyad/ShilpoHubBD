using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ISiteContentRepository
{
    Task<List<SiteContentItem>> GetAllAsync(string? group, bool includeInactive, CancellationToken cancellationToken);
    Task<SiteContentItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(SiteContentItem item, CancellationToken cancellationToken);
    void Remove(SiteContentItem item);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
