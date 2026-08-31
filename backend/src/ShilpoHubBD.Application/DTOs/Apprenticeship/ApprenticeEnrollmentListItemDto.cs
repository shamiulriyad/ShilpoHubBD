namespace ShilpoHubBD.Application.DTOs.Apprenticeship;

public class ApprenticeEnrollmentListItemDto
{
    public Guid Id { get; set; }
    public Guid ProgramId { get; set; }
    public string ProgramTitle { get; set; } = string.Empty;
    public Guid ApprenticeUserId { get; set; }
    public string ApprenticeName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal ProgressPercent { get; set; }
}
