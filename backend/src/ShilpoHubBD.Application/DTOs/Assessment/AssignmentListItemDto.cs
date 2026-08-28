namespace ShilpoHubBD.Application.DTOs.Assessment;

public class AssignmentListItemDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int MaxScore { get; set; }
    public DateTime? DueAt { get; set; }
    public int SubmissionCount { get; set; }
}
