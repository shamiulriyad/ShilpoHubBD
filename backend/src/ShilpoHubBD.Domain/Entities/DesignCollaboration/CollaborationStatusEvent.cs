namespace ShilpoHubBD.Domain.Entities.DesignCollaboration;

public class CollaborationStatusEvent
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public DesignCollaborationProject Project { get; set; } = null!;

    public CollaborationStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
