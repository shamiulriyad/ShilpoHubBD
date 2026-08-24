using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Assessment;

public class AssignmentSubmission
{
    public Guid Id { get; set; }

    public Guid AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    public Guid StudentUserId { get; set; }
    public User Student { get; set; } = null!;

    public string SubmissionText { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;
    public int? Score { get; set; }
    public string? Feedback { get; set; }

    public DateTime SubmittedAt { get; set; }
    public DateTime? GradedAt { get; set; }
    public Guid? GradedByUserId { get; set; }
}
