using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Innovation;

/// <summary>One testing session against a prototype (testing history), with an overall evaluation.</summary>
public class PrototypeTestRun
{
    public Guid Id { get; set; }

    public Guid InnovationPrototypeId { get; set; }
    public InnovationPrototype Prototype { get; set; } = null!;

    public Guid? PrototypeIterationId { get; set; }
    public PrototypeIteration? Iteration { get; set; }

    public int RunNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Environment { get; set; }

    public PrototypeTestRunStatus Status { get; set; } = PrototypeTestRunStatus.Planned;

    public int TotalCases { get; set; }
    public int PassedCases { get; set; }
    public int FailedCases { get; set; }
    public int BlockedCases { get; set; }

    public Guid ExecutedByUserId { get; set; }
    public User ExecutedBy { get; set; } = null!;
    public DateTime? ExecutedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<PrototypeTestResult> Results { get; set; } = new List<PrototypeTestResult>();
}
