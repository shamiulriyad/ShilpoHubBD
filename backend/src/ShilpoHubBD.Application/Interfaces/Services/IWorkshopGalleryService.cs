using ShilpoHubBD.Application.DTOs.Marketplace;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IWorkshopGalleryService
{
    Task<List<WorkshopGalleryItemDto>> GetByProducerIdAsync(Guid producerId, CancellationToken cancellationToken);
    Task<WorkshopGalleryItemDto> AddAsync(Guid producerId, Guid currentUserId, bool isAdmin, CreateWorkshopGalleryItemRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid producerId, Guid itemId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
}
