using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Innovation;

public class SubmissionReview
{
    public Guid Id { get; set; }

    public Guid HeritageInnovationSubmissionId { get; set; }
    public HeritageInnovationSubmission Submission { get; set; } = null!;

    public Guid ReviewerUserId { get; set; }
    public User Reviewer { get; set; } = null!;

    public SubmissionReviewDecision Decision { get; set; } = SubmissionReviewDecision.Comment;
    public int? Score { get; set; }
    public string Comments { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
