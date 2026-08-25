using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Employment;

public class JobApplication
{
    public Guid Id { get; set; }

    public Guid JobListingId { get; set; }
    public JobListing JobListing { get; set; } = null!;

    public Guid ApplicantUserId { get; set; }
    public User Applicant { get; set; } = null!;

    public string CoverMessage { get; set; } = string.Empty;
    public JobApplicationStatus Status { get; set; } = JobApplicationStatus.Pending;

    public DateTime AppliedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? ResponseMessage { get; set; }
}
