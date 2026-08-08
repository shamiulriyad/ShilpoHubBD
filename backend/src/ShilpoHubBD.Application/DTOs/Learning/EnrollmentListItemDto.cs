namespace ShilpoHubBD.Application.DTOs.Learning;

public class EnrollmentListItemDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public Guid ApprenticeId { get; set; }
    public string ApprenticeName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public decimal ProgressPercent { get; set; }
}
