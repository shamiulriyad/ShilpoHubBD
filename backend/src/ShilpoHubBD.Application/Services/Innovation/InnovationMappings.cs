using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Services.Innovation;

internal static class InnovationMappings
{
    // ---- AI Model Builder ----

    public static ExperimentVersionDto ToDto(this InnovationExperimentVersion v) => new()
    {
        Id = v.Id,
        InnovationExperimentId = v.InnovationExperimentId,
        VersionNumber = v.VersionNumber,
        Label = v.Label,
        Notes = v.Notes,
        ConfigJson = v.ConfigJson,
        Framework = v.Framework,
        ArtifactUrl = v.ArtifactUrl,
        IsCurrent = v.IsCurrent,
        CreatedByUserId = v.CreatedByUserId,
        CreatedByName = v.CreatedBy?.FullName ?? string.Empty,
        CreatedAt = v.CreatedAt,
    };

    public static TrainingRunDto ToDto(this TrainingRun r) => new()
    {
        Id = r.Id,
        InnovationExperimentId = r.InnovationExperimentId,
        ExperimentVersionId = r.ExperimentVersionId,
        ExperimentVersionNumber = r.ExperimentVersion?.VersionNumber,
        RunNumber = r.RunNumber,
        Status = r.Status.ToString(),
        DatasetSnapshotName = r.DatasetSnapshotName,
        HyperparametersJson = r.HyperparametersJson,
        MetricsJson = r.MetricsJson,
        PrimaryMetricName = r.PrimaryMetricName,
        PrimaryMetricValue = r.PrimaryMetricValue,
        Notes = r.Notes,
        StartedAt = r.StartedAt,
        CompletedAt = r.CompletedAt,
        TriggeredByUserId = r.TriggeredByUserId,
        TriggeredByName = r.TriggeredBy?.FullName ?? string.Empty,
        CreatedAt = r.CreatedAt,
    };

    public static InnovationExperimentListItemDto ToListItemDto(this InnovationExperiment e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        ModelType = e.ModelType.ToString(),
        Status = e.Status.ToString(),
        Framework = e.Framework,
        OwnerUserId = e.OwnerUserId,
        OwnerName = e.Owner?.FullName ?? string.Empty,
        ResearchProjectId = e.ResearchProjectId,
        HeritageDatasetId = e.HeritageDatasetId,
        VersionCount = e.VersionCount,
        RunCount = e.RunCount,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };

    public static InnovationExperimentDetailDto ToDetailDto(this InnovationExperiment e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Objective = e.Objective,
        Description = e.Description,
        ModelType = e.ModelType.ToString(),
        Framework = e.Framework,
        ConfigJson = e.ConfigJson,
        Status = e.Status.ToString(),
        OwnerUserId = e.OwnerUserId,
        OwnerName = e.Owner?.FullName ?? string.Empty,
        ResearchProjectId = e.ResearchProjectId,
        HeritageDatasetId = e.HeritageDatasetId,
        CurrentVersionId = e.CurrentVersionId,
        VersionCount = e.VersionCount,
        RunCount = e.RunCount,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        Versions = e.Versions.OrderByDescending(v => v.VersionNumber).Select(v => v.ToDto()).ToList(),
        Runs = e.Runs.OrderByDescending(r => r.RunNumber).Select(r => r.ToDto()).ToList(),
    };

    // ---- Preservation Strategy ----

    public static StrategyObjectiveDto ToDto(this StrategyObjective o) => new()
    {
        Id = o.Id,
        PreservationStrategyId = o.PreservationStrategyId,
        Title = o.Title,
        Description = o.Description,
        OrderIndex = o.OrderIndex,
        IsAchieved = o.IsAchieved,
        AchievedAt = o.AchievedAt,
    };

    public static StrategyActionDto ToDto(this StrategyAction a) => new()
    {
        Id = a.Id,
        PreservationStrategyId = a.PreservationStrategyId,
        StrategyObjectiveId = a.StrategyObjectiveId,
        Title = a.Title,
        Description = a.Description,
        Status = a.Status.ToString(),
        OrderIndex = a.OrderIndex,
        StartDate = a.StartDate,
        DueDate = a.DueDate,
        CompletedAt = a.CompletedAt,
        AssignedToUserId = a.AssignedToUserId,
        AssignedToName = a.AssignedTo?.FullName,
    };

    public static PreservationStrategyListItemDto ToListItemDto(this PreservationStrategy s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        Status = s.Status.ToString(),
        OwnerUserId = s.OwnerUserId,
        OwnerName = s.Owner?.FullName ?? string.Empty,
        ResearchProjectId = s.ResearchProjectId,
        ObjectiveCount = s.Objectives.Count,
        ActionCount = s.Actions.Count,
        CompletedActionCount = s.Actions.Count(a => a.Status == StrategyActionStatus.Done),
        StartDate = s.StartDate,
        TargetDate = s.TargetDate,
        UpdatedAt = s.UpdatedAt,
    };

    public static PreservationStrategyDetailDto ToDetailDto(this PreservationStrategy s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        HeritageProblem = s.HeritageProblem,
        ProposedSolution = s.ProposedSolution,
        ExpectedImpact = s.ExpectedImpact,
        EvidenceReferences = s.EvidenceReferences,
        Status = s.Status.ToString(),
        OwnerUserId = s.OwnerUserId,
        OwnerName = s.Owner?.FullName ?? string.Empty,
        ResearchProjectId = s.ResearchProjectId,
        HeritageDatasetId = s.HeritageDatasetId,
        StartDate = s.StartDate,
        TargetDate = s.TargetDate,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt,
        Objectives = s.Objectives.OrderBy(o => o.OrderIndex).Select(o => o.ToDto()).ToList(),
        Actions = s.Actions.OrderBy(a => a.OrderIndex).Select(a => a.ToDto()).ToList(),
    };

    // ---- Prototype ----

    public static PrototypeIterationDto ToDto(this PrototypeIteration i) => new()
    {
        Id = i.Id,
        InnovationPrototypeId = i.InnovationPrototypeId,
        VersionNumber = i.VersionNumber,
        Label = i.Label,
        ChangeSummary = i.ChangeSummary,
        ArtifactUrl = i.ArtifactUrl,
        IsCurrent = i.IsCurrent,
        CreatedByUserId = i.CreatedByUserId,
        CreatedByName = i.CreatedBy?.FullName ?? string.Empty,
        CreatedAt = i.CreatedAt,
    };

    public static PrototypeTestCaseDto ToDto(this PrototypeTestCase c) => new()
    {
        Id = c.Id,
        InnovationPrototypeId = c.InnovationPrototypeId,
        Title = c.Title,
        Description = c.Description,
        Steps = c.Steps,
        ExpectedResult = c.ExpectedResult,
        Priority = c.Priority.ToString(),
        OrderIndex = c.OrderIndex,
        IsActive = c.IsActive,
    };

    public static PrototypeTestResultDto ToDto(this PrototypeTestResult r) => new()
    {
        Id = r.Id,
        PrototypeTestCaseId = r.PrototypeTestCaseId,
        CaseTitle = r.CaseTitle,
        Outcome = r.Outcome.ToString(),
        ActualResult = r.ActualResult,
        Notes = r.Notes,
    };

    public static PrototypeTestRunDto ToDto(this PrototypeTestRun r) => new()
    {
        Id = r.Id,
        InnovationPrototypeId = r.InnovationPrototypeId,
        PrototypeIterationId = r.PrototypeIterationId,
        IterationVersionNumber = r.Iteration?.VersionNumber,
        RunNumber = r.RunNumber,
        Title = r.Title,
        Summary = r.Summary,
        Environment = r.Environment,
        Status = r.Status.ToString(),
        TotalCases = r.TotalCases,
        PassedCases = r.PassedCases,
        FailedCases = r.FailedCases,
        BlockedCases = r.BlockedCases,
        ExecutedByUserId = r.ExecutedByUserId,
        ExecutedByName = r.ExecutedBy?.FullName ?? string.Empty,
        ExecutedAt = r.ExecutedAt,
        CreatedAt = r.CreatedAt,
        Results = r.Results.Select(x => x.ToDto()).ToList(),
    };

    public static PrototypeIssueDto ToDto(this PrototypeIssue i) => new()
    {
        Id = i.Id,
        InnovationPrototypeId = i.InnovationPrototypeId,
        PrototypeTestRunId = i.PrototypeTestRunId,
        Title = i.Title,
        Description = i.Description,
        Severity = i.Severity.ToString(),
        Status = i.Status.ToString(),
        ReportedByUserId = i.ReportedByUserId,
        ReportedByName = i.ReportedBy?.FullName ?? string.Empty,
        ResolvedByUserId = i.ResolvedByUserId,
        ResolvedByName = i.ResolvedBy?.FullName,
        ResolvedAt = i.ResolvedAt,
        Resolution = i.Resolution,
        CreatedAt = i.CreatedAt,
        UpdatedAt = i.UpdatedAt,
    };

    public static InnovationPrototypeListItemDto ToListItemDto(this InnovationPrototype p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Status = p.Status.ToString(),
        Category = p.Category,
        OwnerUserId = p.OwnerUserId,
        OwnerName = p.Owner?.FullName ?? string.Empty,
        ResearchProjectId = p.ResearchProjectId,
        PreservationStrategyId = p.PreservationStrategyId,
        InnovationExperimentId = p.InnovationExperimentId,
        VersionCount = p.VersionCount,
        TestCaseCount = p.TestCases.Count,
        TestRunCount = p.TestRuns.Count,
        OpenIssueCount = p.Issues.Count(i => i.Status != PrototypeIssueStatus.Resolved
            && i.Status != PrototypeIssueStatus.Closed && i.Status != PrototypeIssueStatus.WontFix),
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
    };

    // ---- Submission ----

    public static SubmissionTeamMemberDto ToDto(this SubmissionTeamMember m) => new()
    {
        Id = m.Id,
        UserId = m.UserId,
        UserName = m.User?.FullName ?? string.Empty,
        UserEmail = m.User?.Email ?? string.Empty,
        RoleOnTeam = m.RoleOnTeam,
        AddedByUserId = m.AddedByUserId,
        AddedAt = m.AddedAt,
    };

    public static SubmissionReviewDto ToDto(this SubmissionReview r) => new()
    {
        Id = r.Id,
        ReviewerUserId = r.ReviewerUserId,
        ReviewerName = r.Reviewer?.FullName ?? string.Empty,
        Decision = r.Decision.ToString(),
        Score = r.Score,
        Comments = r.Comments,
        CreatedAt = r.CreatedAt,
    };

    public static SubmissionEventDto ToDto(this SubmissionEvent e) => new()
    {
        Id = e.Id,
        ActorUserId = e.ActorUserId,
        ActorName = e.Actor?.FullName ?? string.Empty,
        EventType = e.EventType.ToString(),
        Summary = e.Summary,
        CreatedAt = e.CreatedAt,
    };

    public static HeritageInnovationSubmissionListItemDto ToListItemDto(this HeritageInnovationSubmission s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        Status = s.Status.ToString(),
        SubmitterUserId = s.SubmitterUserId,
        SubmitterName = s.Submitter?.FullName ?? string.Empty,
        ResearchProjectId = s.ResearchProjectId,
        InnovationPrototypeId = s.InnovationPrototypeId,
        TeamMemberCount = s.TeamMembers.Count,
        ReviewCount = s.Reviews.Count,
        SubmittedAt = s.SubmittedAt,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt,
    };
}
