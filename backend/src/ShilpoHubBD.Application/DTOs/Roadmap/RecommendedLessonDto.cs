namespace ShilpoHubBD.Application.DTOs.Roadmap;

public class RecommendedLessonDto
{
    public Guid CourseLessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
