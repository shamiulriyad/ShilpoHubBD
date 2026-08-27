using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Innovation;

/// <summary>An immutable snapshot of an experiment's model configuration (model versioning).</summary>
public class InnovationExperimentVersion
{
    public Guid Id { get; set; }

    public Guid InnovationExperimentId { get; set; }
    public InnovationExperiment Experiment { get; set; } = null!;

    public int VersionNumber { get; set; }
    public string? Label { get; set; }
    public string Notes { get; set; } = string.Empty;

    public string ConfigJson { get; set; } = string.Empty;
    public string? Framework { get; set; }

    /// <summary>Reference to a stored model artifact (metadata only; nothing is uploaded here).</summary>
    public string? ArtifactUrl { get; set; }

    public bool IsCurrent { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
