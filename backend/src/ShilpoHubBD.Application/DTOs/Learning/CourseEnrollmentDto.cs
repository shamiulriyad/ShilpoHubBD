namespace ShilpoHubBD.Application.DTOs.Learning;

public class CourseEnrollmentDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public Guid ApprenticeId { get; set; }
    public string ApprenticeName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public decimal ProgressPercent { get; set; }
    public List<LessonProgressDto> LessonProgress { get; set; } = new();
}
