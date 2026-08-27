using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Innovation;

/// <summary>An issue found on a prototype, optionally raised during a test run.</summary>
public class PrototypeIssue
{
    public Guid Id { get; set; }

    public Guid InnovationPrototypeId { get; set; }
    public InnovationPrototype Prototype { get; set; } = null!;

    public Guid? PrototypeTestRunId { get; set; }
    public PrototypeTestRun? TestRun { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public PrototypeIssueSeverity Severity { get; set; } = PrototypeIssueSeverity.Medium;
    public PrototypeIssueStatus Status { get; set; } = PrototypeIssueStatus.Open;

    public Guid ReportedByUserId { get; set; }
    public User ReportedBy { get; set; } = null!;

    public Guid? ResolvedByUserId { get; set; }
    public User? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Resolution { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
