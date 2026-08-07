using ShilpoHubBD.Application.DTOs.Community;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Community;

namespace ShilpoHubBD.Application.Services.Community;

public class ProducerFollowService : IProducerFollowService
{
    private readonly IProducerFollowRepository _followRepository;
    private readonly IUserRepository _userRepository;

    public ProducerFollowService(IProducerFollowRepository followRepository, IUserRepository userRepository)
    {
        _followRepository = followRepository;
        _userRepository = userRepository;
    }

    public async Task<List<FollowedProducerDto>> GetMyFollowedProducersAsync(Guid userId, CancellationToken cancellationToken)
    {
        var follows = await _followRepository.GetByFollowerAsync(userId, cancellationToken);
        return follows.Select(f => new FollowedProducerDto
        {
            ProducerId = f.ProducerId,
            ProducerName = f.Producer.FullName,
            FollowedAt = f.CreatedAt,
        }).ToList();
    }

    public async Task FollowAsync(Guid userId, Guid producerId, CancellationToken cancellationToken)
    {
        if (userId == producerId)
        {
            throw new ConflictException("You cannot follow yourself.");
        }

        if (await _userRepository.GetByIdAsync(producerId, cancellationToken) is null)
        {
            throw new NotFoundException("Producer not found.");
        }

        if (await _followRepository.GetAsync(userId, producerId, cancellationToken) is not null)
        {
            return;
        }

        await _followRepository.AddAsync(new ProducerFollow
        {
            Id = Guid.NewGuid(),
            FollowerId = userId,
            ProducerId = producerId,
            CreatedAt = DateTime.UtcNow,
        }, cancellationToken);

        await _followRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task UnfollowAsync(Guid userId, Guid producerId, CancellationToken cancellationToken)
    {
        var follow = await _followRepository.GetAsync(userId, producerId, cancellationToken)
            ?? throw new NotFoundException("You are not following this producer.");

        _followRepository.Remove(follow);
        await _followRepository.SaveChangesAsync(cancellationToken);
    }
}
