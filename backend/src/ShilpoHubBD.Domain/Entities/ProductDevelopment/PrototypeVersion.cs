using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.ProductDevelopment;

public class PrototypeVersion
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public ProductDevelopmentProject Project { get; set; } = null!;

    public int VersionNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public PrototypeStatus Status { get; set; } = PrototypeStatus.Pending;

    public Guid SubmittedByUserId { get; set; }
    public User SubmittedBy { get; set; } = null!;
    public DateTime SubmittedAt { get; set; }

    public DateTime? DecidedAt { get; set; }
    public string? DecisionNotes { get; set; }

    public ICollection<PrototypeFile> Files { get; set; } = new List<PrototypeFile>();
}
