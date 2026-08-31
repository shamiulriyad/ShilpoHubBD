namespace ShilpoHubBD.Application.DTOs.Roadmap;

public class RecommendedLessonPlan
{
    public Guid CourseLessonId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
