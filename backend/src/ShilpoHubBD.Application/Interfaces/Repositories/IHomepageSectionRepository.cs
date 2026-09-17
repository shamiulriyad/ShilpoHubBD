using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IHomepageSectionRepository
{
    Task<List<HomepageSection>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<HomepageSection?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsBySectionKeyAsync(string sectionKey, CancellationToken cancellationToken);
    Task AddAsync(HomepageSection section, CancellationToken cancellationToken);
    void Remove(HomepageSection section);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
