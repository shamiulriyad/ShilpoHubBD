namespace ShilpoHubBD.Application.DTOs.Apprenticeship;

public class ApprenticeEnrollmentDto
{
    public Guid Id { get; set; }
    public Guid ProgramId { get; set; }
    public string ProgramTitle { get; set; } = string.Empty;
    public Guid ApprenticeUserId { get; set; }
    public string ApprenticeName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalMilestones { get; set; }
    public int CompletedMilestones { get; set; }
    public decimal ProgressPercent { get; set; }
    public List<MilestoneProgressDto> Milestones { get; set; } = new();
}
