using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.DesignCollaboration;

public class DesignFile
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public DesignCollaborationProject Project { get; set; } = null!;

    public Guid? RevisionId { get; set; }
    public DesignRevision? Revision { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;

    public Guid UploadedByUserId { get; set; }
    public User UploadedBy { get; set; } = null!;
    public DateTime UploadedAt { get; set; }
}
