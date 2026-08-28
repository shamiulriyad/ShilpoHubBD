using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.DesignCollaboration;

public class DesignRevision
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public DesignCollaborationProject Project { get; set; } = null!;

    public int RevisionNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public RevisionStatus Status { get; set; } = RevisionStatus.Pending;

    public Guid SubmittedByUserId { get; set; }
    public User SubmittedBy { get; set; } = null!;
    public DateTime SubmittedAt { get; set; }

    public DateTime? DecidedAt { get; set; }
    public string? DecisionNotes { get; set; }

    public ICollection<DesignFile> Files { get; set; } = new List<DesignFile>();
}
