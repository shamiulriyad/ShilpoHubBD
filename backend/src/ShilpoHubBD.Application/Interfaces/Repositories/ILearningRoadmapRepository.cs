using ShilpoHubBD.Domain.Entities.Learning;
using ShilpoHubBD.Domain.Entities.Roadmap;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ILearningRoadmapRepository
{
    Task<LearningRoadmap?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<LearningRoadmap?> GetActiveByProfileAsync(Guid academyMemberProfileId, CancellationToken cancellationToken);
    Task<List<LearningRoadmap>> GetByProfileAsync(Guid academyMemberProfileId, CancellationToken cancellationToken);
    Task AddAsync(LearningRoadmap roadmap, CancellationToken cancellationToken);

    Task<RoadmapMilestone?> GetMilestoneByIdAsync(Guid milestoneId, CancellationToken cancellationToken);

    // Best-effort keyword match against published courses (title/category), each with a handful of
    // its lessons — used by the roadmap provider to pick recommendations for a given skill.
    Task<List<Course>> FindCandidateCoursesAsync(string skillName, int courseTake, int lessonTakePerCourse, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
