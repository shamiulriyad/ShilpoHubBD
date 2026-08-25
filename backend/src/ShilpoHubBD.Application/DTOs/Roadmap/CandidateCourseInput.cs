namespace ShilpoHubBD.Application.DTOs.Roadmap;

public class CandidateCourseInput
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<CandidateLessonInput> Lessons { get; set; } = new();
}
