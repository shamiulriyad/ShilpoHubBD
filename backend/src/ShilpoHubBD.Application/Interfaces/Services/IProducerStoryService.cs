using ShilpoHubBD.Application.DTOs.Marketplace;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IProducerStoryService
{
    Task<ProducerStoryDto> GetByProducerIdAsync(Guid producerId, CancellationToken cancellationToken);
    Task<ProducerStoryDto> UpsertAsync(Guid producerId, Guid currentUserId, bool isAdmin, UpsertProducerStoryRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid producerId, CancellationToken cancellationToken);
}
