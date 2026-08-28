namespace ShilpoHubBD.Application.DTOs.Roadmap;

public class CandidateLessonInput
{
    public Guid CourseLessonId { get; set; }
    public string Title { get; set; } = string.Empty;
}
