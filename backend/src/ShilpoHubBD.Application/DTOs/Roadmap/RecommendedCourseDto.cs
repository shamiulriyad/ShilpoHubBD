namespace ShilpoHubBD.Application.DTOs.Roadmap;

public class RecommendedCourseDto
{
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
