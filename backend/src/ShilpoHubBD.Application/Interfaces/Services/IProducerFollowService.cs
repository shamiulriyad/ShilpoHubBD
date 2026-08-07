using ShilpoHubBD.Application.DTOs.Community;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IProducerFollowService
{
    Task<List<FollowedProducerDto>> GetMyFollowedProducersAsync(Guid userId, CancellationToken cancellationToken);
    Task FollowAsync(Guid userId, Guid producerId, CancellationToken cancellationToken);
    Task UnfollowAsync(Guid userId, Guid producerId, CancellationToken cancellationToken);
}
