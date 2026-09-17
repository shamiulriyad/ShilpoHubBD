using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IAnnouncementRepository
{
    Task<List<Announcement>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken);
    Task<Announcement?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Announcement announcement, CancellationToken cancellationToken);
    void Remove(Announcement announcement);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
