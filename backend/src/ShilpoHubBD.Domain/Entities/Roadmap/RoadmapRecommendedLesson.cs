using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Domain.Entities.Roadmap;

public class RoadmapRecommendedLesson
{
    public Guid Id { get; set; }

    public Guid RoadmapMilestoneId { get; set; }
    public RoadmapMilestone RoadmapMilestone { get; set; } = null!;

    public Guid CourseLessonId { get; set; }
    public CourseLesson CourseLesson { get; set; } = null!;

    public string Reason { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
