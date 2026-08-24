using ShilpoHubBD.Application.DTOs.Roadmap;

namespace ShilpoHubBD.Application.Interfaces.Services;

// Decides which skills form a learner's roadmap, what level each targets, and which of the supplied
// candidate courses/lessons to recommend. Kept as an abstraction so RuleBasedLearningRoadmapProvider
// can later be replaced by an AI-backed planner without touching LearningRoadmapService.
public interface ILearningRoadmapProvider
{
    Task<RoadmapGenerationResult> GenerateAsync(RoadmapGenerationInput input, CancellationToken cancellationToken);
}
