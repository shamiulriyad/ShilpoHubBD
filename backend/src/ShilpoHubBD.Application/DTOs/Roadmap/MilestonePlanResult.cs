using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.DTOs.Roadmap;

public class MilestonePlanResult
{
    public Guid HeritageSkillId { get; set; }
    public SkillLevel TargetLevel { get; set; }
    public bool IsAlreadyCompleted { get; set; }
    public List<RecommendedCoursePlan> RecommendedCourses { get; set; } = new();
    public List<RecommendedLessonPlan> RecommendedLessons { get; set; } = new();
}
