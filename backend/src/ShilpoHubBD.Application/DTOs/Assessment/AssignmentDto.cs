namespace ShilpoHubBD.Application.DTOs.Assessment;

public class AssignmentDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxScore { get; set; }
    public DateTime? DueAt { get; set; }
    public int SubmissionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
