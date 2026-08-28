using ShilpoHubBD.Domain.Entities.HeritageDatabase;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Domain.Entities.Innovation;

/// <summary>
/// An AI Model Builder experiment. Stores model/config/training metadata only -- no training is run.
/// Kept provider-agnostic so a real ML pipeline can be attached later.
/// </summary>
public class InnovationExperiment
{
    public Guid Id { get; set; }

    public Guid OwnerUserId { get; set; }
    public User Owner { get; set; } = null!;

    public Guid? ResearchProjectId { get; set; }
    public ResearchProject? ResearchProject { get; set; }

    public Guid? HeritageDatasetId { get; set; }
    public HeritageDataset? HeritageDataset { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string? Description { get; set; }

    public InnovationModelType ModelType { get; set; } = InnovationModelType.Other;
    public string? Framework { get; set; }

    /// <summary>Model configuration metadata (hyper-parameters, features, ...), free-form JSON.</summary>
    public string? ConfigJson { get; set; }

    public InnovationExperimentStatus Status { get; set; } = InnovationExperimentStatus.Draft;

    public int VersionCount { get; set; }
    public int RunCount { get; set; }
    public Guid? CurrentVersionId { get; set; }
    public InnovationExperimentVersion? CurrentVersion { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<InnovationExperimentVersion> Versions { get; set; } = new List<InnovationExperimentVersion>();
    public ICollection<TrainingRun> Runs { get; set; } = new List<TrainingRun>();
}
