using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IWorkshopGalleryRepository
{
    Task<List<WorkshopGalleryItem>> GetByProducerIdAsync(Guid producerId, CancellationToken cancellationToken);
    Task<WorkshopGalleryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(WorkshopGalleryItem item, CancellationToken cancellationToken);
    void Remove(WorkshopGalleryItem item);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
