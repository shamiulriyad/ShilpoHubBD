namespace ShilpoHubBD.Application.DTOs.Learning;

public class CourseListItemDto
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string Status { get; set; } = string.Empty;
    public int LessonCount { get; set; }
    public int? MaxApprentices { get; set; }
    public int ActiveEnrollmentCount { get; set; }
}
