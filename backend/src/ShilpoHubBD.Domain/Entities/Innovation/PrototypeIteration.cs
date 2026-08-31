using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Innovation;

/// <summary>A versioned iteration of a prototype.</summary>
public class PrototypeIteration
{
    public Guid Id { get; set; }

    public Guid InnovationPrototypeId { get; set; }
    public InnovationPrototype Prototype { get; set; } = null!;

    public int VersionNumber { get; set; }
    public string? Label { get; set; }
    public string ChangeSummary { get; set; } = string.Empty;
    public string? ArtifactUrl { get; set; }

    public bool IsCurrent { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
