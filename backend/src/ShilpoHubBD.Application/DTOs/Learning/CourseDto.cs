namespace ShilpoHubBD.Application.DTOs.Learning;

public class CourseDto
{
    public Guid Id { get; set; }
    public Guid MentorId { get; set; }
    public string MentorName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? MaxApprentices { get; set; }
    public int ActiveEnrollmentCount { get; set; }
    public List<CourseLessonDto> Lessons { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}
