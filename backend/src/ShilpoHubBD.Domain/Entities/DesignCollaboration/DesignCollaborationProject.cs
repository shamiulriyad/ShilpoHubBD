using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.DesignCollaboration;

public class DesignCollaborationProject
{
    public Guid Id { get; set; }

    public Guid BusinessPartnerId { get; set; }
    public User BusinessPartner { get; set; } = null!;

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DesignRequirements { get; set; } = string.Empty;

    public CollaborationStatus Status { get; set; } = CollaborationStatus.Invited;
    public DateTime? RespondedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<DesignFile> Files { get; set; } = new List<DesignFile>();
    public ICollection<DesignComment> Comments { get; set; } = new List<DesignComment>();
    public ICollection<DesignRevision> Revisions { get; set; } = new List<DesignRevision>();
    public ICollection<CollaborationStatusEvent> StatusHistory { get; set; } = new List<CollaborationStatusEvent>();
}
