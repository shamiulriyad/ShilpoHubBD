using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Domain.Entities.Innovation;

/// <summary>A heritage-innovation prototype under test. Optionally realises a strategy or an experiment.</summary>
public class InnovationPrototype
{
    public Guid Id { get; set; }

    public Guid OwnerUserId { get; set; }
    public User Owner { get; set; } = null!;

    public Guid? ResearchProjectId { get; set; }
    public ResearchProject? ResearchProject { get; set; }

    public Guid? PreservationStrategyId { get; set; }
    public PreservationStrategy? PreservationStrategy { get; set; }

    public Guid? InnovationExperimentId { get; set; }
    public InnovationExperiment? InnovationExperiment { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }

    public InnovationPrototypeStatus Status { get; set; } = InnovationPrototypeStatus.Concept;

    public int VersionCount { get; set; }
    public Guid? CurrentIterationId { get; set; }
    public PrototypeIteration? CurrentIteration { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<PrototypeIteration> Iterations { get; set; } = new List<PrototypeIteration>();
    public ICollection<PrototypeTestCase> TestCases { get; set; } = new List<PrototypeTestCase>();
    public ICollection<PrototypeTestRun> TestRuns { get; set; } = new List<PrototypeTestRun>();
    public ICollection<PrototypeIssue> Issues { get; set; } = new List<PrototypeIssue>();
}
