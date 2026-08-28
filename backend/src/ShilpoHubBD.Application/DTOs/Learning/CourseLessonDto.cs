namespace ShilpoHubBD.Application.DTOs.Learning;

public class CourseLessonDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid? ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public int DisplayOrder { get; set; }
}
