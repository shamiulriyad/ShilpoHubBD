namespace ShilpoHubBD.Domain.Entities.Apprenticeship;

public class ApprenticeMilestoneProgress
{
    public Guid Id { get; set; }

    public Guid EnrollmentId { get; set; }
    public ApprenticeEnrollment Enrollment { get; set; } = null!;

    public Guid MilestoneId { get; set; }
    public TrainingMilestone Milestone { get; set; } = null!;

    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
}
