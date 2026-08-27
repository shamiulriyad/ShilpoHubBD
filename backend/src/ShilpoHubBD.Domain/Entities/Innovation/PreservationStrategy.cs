using ShilpoHubBD.Domain.Entities.HeritageDatabase;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Domain.Entities.Innovation;

/// <summary>A designed heritage-preservation strategy: problem, solution, objectives, actions, timeline.</summary>
public class PreservationStrategy
{
    public Guid Id { get; set; }

    public Guid OwnerUserId { get; set; }
    public User Owner { get; set; } = null!;

    public Guid? ResearchProjectId { get; set; }
    public ResearchProject? ResearchProject { get; set; }

    public Guid? HeritageDatasetId { get; set; }
    public HeritageDataset? HeritageDataset { get; set; }

    public string Title { get; set; } = string.Empty;
    public string HeritageProblem { get; set; } = string.Empty;
    public string ProposedSolution { get; set; } = string.Empty;
    public string? ExpectedImpact { get; set; }

    /// <summary>Free-form evidence / data references (citations, URLs, dataset notes).</summary>
    public string? EvidenceReferences { get; set; }

    public PreservationStrategyStatus Status { get; set; } = PreservationStrategyStatus.Draft;

    public DateTime? StartDate { get; set; }
    public DateTime? TargetDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<StrategyObjective> Objectives { get; set; } = new List<StrategyObjective>();
    public ICollection<StrategyAction> Actions { get; set; } = new List<StrategyAction>();
}
