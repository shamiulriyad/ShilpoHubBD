using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Domain.Entities.Roadmap;

public class RoadmapMilestone
{
    public Guid Id { get; set; }

    public Guid LearningRoadmapId { get; set; }
    public LearningRoadmap LearningRoadmap { get; set; } = null!;

    public Guid HeritageSkillId { get; set; }
    public HeritageSkill HeritageSkill { get; set; } = null!;

    public SkillLevel TargetLevel { get; set; }
    public int DisplayOrder { get; set; }

    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<RoadmapRecommendedCourse> RecommendedCourses { get; set; } = new List<RoadmapRecommendedCourse>();
    public ICollection<RoadmapRecommendedLesson> RecommendedLessons { get; set; } = new List<RoadmapRecommendedLesson>();
}
