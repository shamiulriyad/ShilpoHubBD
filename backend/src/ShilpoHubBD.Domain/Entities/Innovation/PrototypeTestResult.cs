namespace ShilpoHubBD.Domain.Entities.Innovation;

public class PrototypeTestResult
{
    public Guid Id { get; set; }

    public Guid PrototypeTestRunId { get; set; }
    public PrototypeTestRun TestRun { get; set; } = null!;

    public Guid? PrototypeTestCaseId { get; set; }
    public PrototypeTestCase? TestCase { get; set; }

    /// <summary>Snapshot of the case title at execution time (results survive case edits/deletes).</summary>
    public string CaseTitle { get; set; } = string.Empty;

    public TestResultOutcome Outcome { get; set; } = TestResultOutcome.Skipped;
    public string? ActualResult { get; set; }
    public string? Notes { get; set; }
}
