using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Innovation;

/// <summary>Append-only submission history.</summary>
public class SubmissionEvent
{
    public Guid Id { get; set; }

    public Guid HeritageInnovationSubmissionId { get; set; }
    public HeritageInnovationSubmission Submission { get; set; } = null!;

    public Guid ActorUserId { get; set; }
    public User Actor { get; set; } = null!;

    public SubmissionEventType EventType { get; set; }
    public string Summary { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
