using ShilpoHubBD.Application.DTOs.Roadmap;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ILearningRoadmapService
{
    Task<LearningRoadmapDto> CreateAsync(Guid userId, CreateRoadmapRequest request, CancellationToken cancellationToken);

    Task<LearningRoadmapDto> GetActiveAsync(Guid userId, CancellationToken cancellationToken);

    Task<LearningRoadmapDto> GetByIdAsync(Guid userId, Guid roadmapId, CancellationToken cancellationToken);

    Task<List<LearningRoadmapListItemDto>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken);

    Task<LearningRoadmapDto> RefreshProgressAsync(Guid userId, Guid roadmapId, CancellationToken cancellationToken);

    Task<LearningRoadmapDto> CompleteMilestoneAsync(Guid userId, Guid roadmapId, Guid milestoneId, CancellationToken cancellationToken);
}
