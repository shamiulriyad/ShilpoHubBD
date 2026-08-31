using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Domain.Entities.Roadmap;

public class RoadmapRecommendedCourse
{
    public Guid Id { get; set; }

    public Guid RoadmapMilestoneId { get; set; }
    public RoadmapMilestone RoadmapMilestone { get; set; } = null!;

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public string Reason { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
