namespace ShilpoHubBD.Application.DTOs.Roadmap;

public class RoadmapMilestoneDto
{
    public Guid Id { get; set; }
    public Guid HeritageSkillId { get; set; }
    public string HeritageSkillName { get; set; } = string.Empty;
    public string TargetLevel { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<RecommendedCourseDto> RecommendedCourses { get; set; } = new();
    public List<RecommendedLessonDto> RecommendedLessons { get; set; } = new();
}
