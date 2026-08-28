using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Apprenticeship;

public class ApprenticeEnrollment
{
    public Guid Id { get; set; }

    public Guid ProgramId { get; set; }
    public ApprenticeshipProgram Program { get; set; } = null!;

    public Guid ApprenticeUserId { get; set; }
    public User Apprentice { get; set; } = null!;

    public Guid? ApplicationId { get; set; }
    public ProgramApplication? Application { get; set; }

    public ApprenticeEnrollmentStatus Status { get; set; } = ApprenticeEnrollmentStatus.Active;

    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<ApprenticeMilestoneProgress> MilestoneProgress { get; set; } = new List<ApprenticeMilestoneProgress>();
}
