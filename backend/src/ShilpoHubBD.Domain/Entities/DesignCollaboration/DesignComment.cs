using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.DesignCollaboration;

public class DesignComment
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public DesignCollaborationProject Project { get; set; } = null!;

    public Guid AuthorUserId { get; set; }
    public User Author { get; set; } = null!;

    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
