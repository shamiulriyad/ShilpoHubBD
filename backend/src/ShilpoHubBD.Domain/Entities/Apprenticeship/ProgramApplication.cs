using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Apprenticeship;

public class ProgramApplication
{
    public Guid Id { get; set; }

    public Guid ProgramId { get; set; }
    public ApprenticeshipProgram Program { get; set; } = null!;

    public Guid ApplicantUserId { get; set; }
    public User Applicant { get; set; } = null!;

    public string Message { get; set; } = string.Empty;

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    public DateTime AppliedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? ResponseMessage { get; set; }
}
