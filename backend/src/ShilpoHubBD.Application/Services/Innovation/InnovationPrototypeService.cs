using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Services.Innovation;

public class InnovationPrototypeService : IInnovationPrototypeService
{
    private readonly IInnovationPrototypeRepository _repository;
    private readonly IInnovationLinkResolver _links;

    public InnovationPrototypeService(IInnovationPrototypeRepository repository, IInnovationLinkResolver links)
    {
        _repository = repository;
        _links = links;
    }

    public async Task<PagedResult<InnovationPrototypeListItemDto>> GetMineAsync(
        Guid userId, InnovationPrototypeQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 12 : query.PageSize;

        var (items, totalCount) = await _repository.GetPagedForOwnerAsync(userId, query, cancellationToken);
        return new PagedResult<InnovationPrototypeListItemDto>
        {
            Items = items.Select(p => p.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<InnovationPrototypeDetailDto> GetByIdAsync(Guid userId, Guid prototypeId, CancellationToken cancellationToken)
    {
        var prototype = await LoadOwnedDetailAsync(userId, prototypeId, cancellationToken);
        return await BuildDetailAsync(prototype, cancellationToken);
    }

    public async Task<InnovationPrototypeDetailDto> CreateAsync(
        Guid userId, bool isResearcher, CreateInnovationPrototypeRequest request, CancellationToken cancellationToken)
    {
        await ValidateLinksAsync(userId, request.ResearchProjectId, request.PreservationStrategyId,
            request.InnovationExperimentId, cancellationToken);

        var now = DateTime.UtcNow;
        var prototype = new InnovationPrototype
        {
            Id = Guid.NewGuid(),
            OwnerUserId = userId,
            ResearchProjectId = request.ResearchProjectId,
            PreservationStrategyId = request.PreservationStrategyId,
            InnovationExperimentId = request.InnovationExperimentId,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Category = request.Category?.Trim(),
            Status = InnovationPrototypeStatus.Concept,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddAsync(prototype, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return await BuildDetailAsync((await _repository.GetDetailAsync(prototype.Id, cancellationToken))!, cancellationToken);
    }

    public async Task<InnovationPrototypeDetailDto> UpdateAsync(
        Guid userId, bool isResearcher, Guid prototypeId, UpdateInnovationPrototypeRequest request, CancellationToken cancellationToken)
    {
        var prototype = await LoadOwnedDetailAsync(userId, prototypeId, cancellationToken);

        if (!Enum.TryParse<InnovationPrototypeStatus>(request.Status, true, out var status))
        {
            throw new ConflictException("Status is not a valid prototype status.");
        }

        await ValidateLinksAsync(userId, request.ResearchProjectId, request.PreservationStrategyId,
            request.InnovationExperimentId, cancellationToken);

        prototype.Name = request.Name.Trim();
        prototype.Description = request.Description.Trim();
        prototype.Category = request.Category?.Trim();
        prototype.Status = status;
        prototype.ResearchProjectId = request.ResearchProjectId;
        prototype.PreservationStrategyId = request.PreservationStrategyId;
        prototype.InnovationExperimentId = request.InnovationExperimentId;
        prototype.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return await BuildDetailAsync(prototype, cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, Guid prototypeId, CancellationToken cancellationToken)
    {
        var prototype = await _repository.GetByIdAsync(prototypeId, cancellationToken)
            ?? throw new NotFoundException("Prototype not found.");
        RequireOwner(prototype, userId);

        _repository.Remove(prototype);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- iterations ----

    public async Task<PrototypeIterationDto> AddIterationAsync(
        Guid userId, Guid prototypeId, CreatePrototypeIterationRequest request, CancellationToken cancellationToken)
    {
        var prototype = await LoadOwnedDetailAsync(userId, prototypeId, cancellationToken);

        var nextNumber = await _repository.GetMaxIterationNumberAsync(prototypeId, cancellationToken) + 1;
        var now = DateTime.UtcNow;
        var iteration = new PrototypeIteration
        {
            Id = Guid.NewGuid(),
            InnovationPrototypeId = prototypeId,
            VersionNumber = nextNumber,
            Label = string.IsNullOrWhiteSpace(request.Label) ? $"v{nextNumber}" : request.Label.Trim(),
            ChangeSummary = request.ChangeSummary.Trim(),
            ArtifactUrl = request.ArtifactUrl?.Trim(),
            IsCurrent = request.SetAsCurrent,
            CreatedByUserId = userId,
            CreatedAt = now,
        };

        if (request.SetAsCurrent)
        {
            foreach (var existing in prototype.Iterations.Where(i => i.IsCurrent))
            {
                existing.IsCurrent = false;
            }
        }

        await _repository.AddIterationAsync(iteration, cancellationToken);
        prototype.VersionCount += 1;
        prototype.UpdatedAt = now;
        if (request.SetAsCurrent)
        {
            prototype.CurrentIterationId = iteration.Id;
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetIterationByIdAsync(iteration.Id, cancellationToken))!.ToDto();
    }

    // ---- test cases ----

    public async Task<PrototypeTestCaseDto> AddTestCaseAsync(
        Guid userId, Guid prototypeId, CreatePrototypeTestCaseRequest request, CancellationToken cancellationToken)
    {
        var prototype = await LoadOwnedAsync(userId, prototypeId, cancellationToken);
        var priority = ParsePriority(request.Priority) ?? TestCasePriority.Medium;

        var now = DateTime.UtcNow;
        var testCase = new PrototypeTestCase
        {
            Id = Guid.NewGuid(),
            InnovationPrototypeId = prototypeId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Steps = request.Steps?.Trim(),
            ExpectedResult = request.ExpectedResult.Trim(),
            Priority = priority,
            OrderIndex = request.OrderIndex,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddTestCaseAsync(testCase, cancellationToken);
        prototype.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);
        return testCase.ToDto();
    }

    public async Task<PrototypeTestCaseDto> UpdateTestCaseAsync(
        Guid userId, Guid prototypeId, Guid testCaseId, UpdatePrototypeTestCaseRequest request, CancellationToken cancellationToken)
    {
        await LoadOwnedAsync(userId, prototypeId, cancellationToken);
        var testCase = await _repository.GetTestCaseByIdAsync(testCaseId, cancellationToken);
        if (testCase is null || testCase.InnovationPrototypeId != prototypeId)
        {
            throw new NotFoundException("Test case not found.");
        }

        var priority = ParsePriority(request.Priority)
            ?? throw new ConflictException("Priority must be one of: Low, Medium, High, Critical.");

        testCase.Title = request.Title.Trim();
        testCase.Description = request.Description?.Trim();
        testCase.Steps = request.Steps?.Trim();
        testCase.ExpectedResult = request.ExpectedResult.Trim();
        testCase.Priority = priority;
        testCase.OrderIndex = request.OrderIndex;
        testCase.IsActive = request.IsActive;
        testCase.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return testCase.ToDto();
    }

    public async Task DeleteTestCaseAsync(Guid userId, Guid prototypeId, Guid testCaseId, CancellationToken cancellationToken)
    {
        await LoadOwnedAsync(userId, prototypeId, cancellationToken);
        var testCase = await _repository.GetTestCaseByIdAsync(testCaseId, cancellationToken);
        if (testCase is null || testCase.InnovationPrototypeId != prototypeId)
        {
            throw new NotFoundException("Test case not found.");
        }

        _repository.RemoveTestCase(testCase);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- test runs ----

    public async Task<PagedResult<PrototypeTestRunDto>> GetTestRunsAsync(
        Guid userId, Guid prototypeId, int page, int pageSize, CancellationToken cancellationToken)
    {
        await LoadOwnedAsync(userId, prototypeId, cancellationToken);
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var (items, totalCount) = await _repository.GetTestRunsAsync(prototypeId, page, pageSize, cancellationToken);
        return new PagedResult<PrototypeTestRunDto>
        {
            Items = items.Select(r => r.ToDto()).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<PrototypeTestRunDto> GetTestRunAsync(
        Guid userId, Guid prototypeId, Guid testRunId, CancellationToken cancellationToken)
    {
        await LoadOwnedAsync(userId, prototypeId, cancellationToken);
        return (await LoadTestRunAsync(prototypeId, testRunId, cancellationToken)).ToDto();
    }

    public async Task<PrototypeTestRunDto> CreateTestRunAsync(
        Guid userId, Guid prototypeId, CreatePrototypeTestRunRequest request, CancellationToken cancellationToken)
    {
        var prototype = await LoadOwnedDetailAsync(userId, prototypeId, cancellationToken);
        var iterationId = ValidateIteration(prototype, request.PrototypeIterationId);
        var testCases = await _repository.GetTestCasesAsync(prototypeId, cancellationToken);

        var nextNumber = await _repository.GetMaxTestRunNumberAsync(prototypeId, cancellationToken) + 1;
        var now = DateTime.UtcNow;
        var run = new PrototypeTestRun
        {
            Id = Guid.NewGuid(),
            InnovationPrototypeId = prototypeId,
            PrototypeIterationId = iterationId,
            RunNumber = nextNumber,
            Title = request.Title.Trim(),
            Environment = request.Environment?.Trim(),
            ExecutedByUserId = userId,
            ExecutedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        BuildResults(run, request.Results, testCases);
        ApplyRunTotals(run);
        run.Status = DeriveRunStatus(run);

        await _repository.AddTestRunAsync(run, cancellationToken);
        prototype.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);

        return (await LoadTestRunAsync(prototypeId, run.Id, cancellationToken)).ToDto();
    }

    public async Task<PrototypeTestRunDto> UpdateTestRunAsync(
        Guid userId, Guid prototypeId, Guid testRunId, UpdatePrototypeTestRunRequest request, CancellationToken cancellationToken)
    {
        var prototype = await LoadOwnedDetailAsync(userId, prototypeId, cancellationToken);
        var run = await LoadTestRunAsync(prototypeId, testRunId, cancellationToken);

        if (!Enum.TryParse<PrototypeTestRunStatus>(request.Status, true, out var status))
        {
            throw new ConflictException("Status must be one of: Planned, InProgress, Passed, Failed, Blocked.");
        }

        var iterationId = ValidateIteration(prototype, request.PrototypeIterationId);
        var testCases = await _repository.GetTestCasesAsync(prototypeId, cancellationToken);

        run.Title = request.Title.Trim();
        run.Summary = request.Summary?.Trim();
        run.Environment = request.Environment?.Trim();
        run.PrototypeIterationId = iterationId;
        run.Status = status;
        run.ExecutedAt = request.ExecutedAt ?? run.ExecutedAt;
        run.UpdatedAt = DateTime.UtcNow;

        _repository.RemoveTestResults(run.Results.ToList());
        run.Results.Clear();
        BuildResults(run, request.Results, testCases);
        ApplyRunTotals(run);

        await _repository.SaveChangesAsync(cancellationToken);
        return (await LoadTestRunAsync(prototypeId, run.Id, cancellationToken)).ToDto();
    }

    public async Task DeleteTestRunAsync(Guid userId, Guid prototypeId, Guid testRunId, CancellationToken cancellationToken)
    {
        await LoadOwnedAsync(userId, prototypeId, cancellationToken);
        var run = await LoadTestRunAsync(prototypeId, testRunId, cancellationToken);
        _repository.RemoveTestRun(run);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- issues ----

    public async Task<PagedResult<PrototypeIssueDto>> GetIssuesAsync(
        Guid userId, Guid prototypeId, string? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        await LoadOwnedAsync(userId, prototypeId, cancellationToken);
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var (items, totalCount) = await _repository.GetIssuesAsync(prototypeId, status, page, pageSize, cancellationToken);
        return new PagedResult<PrototypeIssueDto>
        {
            Items = items.Select(i => i.ToDto()).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<PrototypeIssueDto> CreateIssueAsync(
        Guid userId, Guid prototypeId, CreatePrototypeIssueRequest request, CancellationToken cancellationToken)
    {
        var prototype = await LoadOwnedAsync(userId, prototypeId, cancellationToken);
        var severity = ParseSeverity(request.Severity) ?? PrototypeIssueSeverity.Medium;

        if (request.PrototypeTestRunId.HasValue)
        {
            _ = await LoadTestRunAsync(prototypeId, request.PrototypeTestRunId.Value, cancellationToken);
        }

        var now = DateTime.UtcNow;
        var issue = new PrototypeIssue
        {
            Id = Guid.NewGuid(),
            InnovationPrototypeId = prototypeId,
            PrototypeTestRunId = request.PrototypeTestRunId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = severity,
            Status = PrototypeIssueStatus.Open,
            ReportedByUserId = userId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddIssueAsync(issue, cancellationToken);
        prototype.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetIssueByIdAsync(issue.Id, cancellationToken))!.ToDto();
    }

    public async Task<PrototypeIssueDto> UpdateIssueAsync(
        Guid userId, Guid prototypeId, Guid issueId, UpdatePrototypeIssueRequest request, CancellationToken cancellationToken)
    {
        await LoadOwnedAsync(userId, prototypeId, cancellationToken);
        var issue = await _repository.GetIssueByIdAsync(issueId, cancellationToken);
        if (issue is null || issue.InnovationPrototypeId != prototypeId)
        {
            throw new NotFoundException("Issue not found.");
        }

        var severity = ParseSeverity(request.Severity)
            ?? throw new ConflictException("Severity must be one of: Low, Medium, High, Critical.");
        if (!Enum.TryParse<PrototypeIssueStatus>(request.Status, true, out var status))
        {
            throw new ConflictException("Status must be one of: Open, InProgress, Resolved, WontFix, Closed.");
        }

        var nowResolved = status is PrototypeIssueStatus.Resolved or PrototypeIssueStatus.WontFix or PrototypeIssueStatus.Closed;
        var wasResolved = issue.Status is PrototypeIssueStatus.Resolved or PrototypeIssueStatus.WontFix or PrototypeIssueStatus.Closed;

        issue.Title = request.Title.Trim();
        issue.Description = request.Description.Trim();
        issue.Severity = severity;
        issue.Status = status;
        issue.Resolution = request.Resolution?.Trim();
        if (nowResolved && !wasResolved)
        {
            issue.ResolvedByUserId = userId;
            issue.ResolvedAt = DateTime.UtcNow;
        }
        else if (!nowResolved)
        {
            issue.ResolvedByUserId = null;
            issue.ResolvedAt = null;
        }

        issue.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetIssueByIdAsync(issue.Id, cancellationToken))!.ToDto();
    }

    public async Task DeleteIssueAsync(Guid userId, Guid prototypeId, Guid issueId, CancellationToken cancellationToken)
    {
        await LoadOwnedAsync(userId, prototypeId, cancellationToken);
        var issue = await _repository.GetIssueByIdAsync(issueId, cancellationToken);
        if (issue is null || issue.InnovationPrototypeId != prototypeId)
        {
            throw new NotFoundException("Issue not found.");
        }

        _repository.RemoveIssue(issue);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- helpers ----

    private async Task<InnovationPrototypeDetailDto> BuildDetailAsync(InnovationPrototype prototype, CancellationToken cancellationToken)
    {
        var dto = new InnovationPrototypeDetailDto
        {
            Id = prototype.Id,
            Name = prototype.Name,
            Description = prototype.Description,
            Category = prototype.Category,
            Status = prototype.Status.ToString(),
            OwnerUserId = prototype.OwnerUserId,
            OwnerName = prototype.Owner?.FullName ?? string.Empty,
            ResearchProjectId = prototype.ResearchProjectId,
            PreservationStrategyId = prototype.PreservationStrategyId,
            InnovationExperimentId = prototype.InnovationExperimentId,
            CurrentIterationId = prototype.CurrentIterationId,
            VersionCount = prototype.VersionCount,
            CreatedAt = prototype.CreatedAt,
            UpdatedAt = prototype.UpdatedAt,
            Iterations = prototype.Iterations.OrderByDescending(i => i.VersionNumber).Select(i => i.ToDto()).ToList(),
            TestCases = prototype.TestCases.OrderBy(c => c.OrderIndex).Select(c => c.ToDto()).ToList(),
        };

        var (_, runCount) = await _repository.GetTestRunsAsync(prototype.Id, 1, 1, cancellationToken);
        dto.TestRunCount = runCount;
        dto.OpenIssueCount = await _repository.CountOpenIssuesAsync(prototype.Id, cancellationToken);
        return dto;
    }

    private async Task<InnovationPrototype> LoadOwnedAsync(Guid userId, Guid prototypeId, CancellationToken cancellationToken)
    {
        var prototype = await _repository.GetByIdAsync(prototypeId, cancellationToken)
            ?? throw new NotFoundException("Prototype not found.");
        RequireOwner(prototype, userId);
        return prototype;
    }

    private async Task<InnovationPrototype> LoadOwnedDetailAsync(Guid userId, Guid prototypeId, CancellationToken cancellationToken)
    {
        var prototype = await _repository.GetDetailAsync(prototypeId, cancellationToken)
            ?? throw new NotFoundException("Prototype not found.");
        RequireOwner(prototype, userId);
        return prototype;
    }

    private async Task<PrototypeTestRun> LoadTestRunAsync(Guid prototypeId, Guid testRunId, CancellationToken cancellationToken)
    {
        var run = await _repository.GetTestRunByIdAsync(testRunId, cancellationToken);
        if (run is null || run.InnovationPrototypeId != prototypeId)
        {
            throw new NotFoundException("Test run not found.");
        }

        return run;
    }

    private static void RequireOwner(InnovationPrototype prototype, Guid userId)
    {
        if (prototype.OwnerUserId != userId)
        {
            throw new NotFoundException("Prototype not found.");
        }
    }

    private static Guid? ValidateIteration(InnovationPrototype prototype, Guid? iterationId)
    {
        if (iterationId.HasValue && prototype.Iterations.All(i => i.Id != iterationId.Value))
        {
            throw new NotFoundException("Prototype iteration not found.");
        }

        return iterationId;
    }

    private static void BuildResults(
        PrototypeTestRun run, List<TestResultInputDto> inputs, List<PrototypeTestCase> testCases)
    {
        var caseById = testCases.ToDictionary(c => c.Id);

        foreach (var input in inputs)
        {
            if (!Enum.TryParse<TestResultOutcome>(input.Outcome, true, out var outcome))
            {
                throw new ConflictException("Outcome must be one of: Pass, Fail, Blocked, Skipped.");
            }

            string caseTitle;
            if (input.PrototypeTestCaseId.HasValue)
            {
                if (!caseById.TryGetValue(input.PrototypeTestCaseId.Value, out var testCase))
                {
                    throw new NotFoundException("A result references a test case that does not belong to this prototype.");
                }

                caseTitle = testCase.Title;
            }
            else
            {
                caseTitle = string.IsNullOrWhiteSpace(input.CaseTitle)
                    ? throw new ConflictException("Each ad-hoc result needs a case title.")
                    : input.CaseTitle!.Trim();
            }

            run.Results.Add(new PrototypeTestResult
            {
                Id = Guid.NewGuid(),
                PrototypeTestRunId = run.Id,
                PrototypeTestCaseId = input.PrototypeTestCaseId,
                CaseTitle = caseTitle,
                Outcome = outcome,
                ActualResult = input.ActualResult?.Trim(),
                Notes = input.Notes?.Trim(),
            });
        }
    }

    private static void ApplyRunTotals(PrototypeTestRun run)
    {
        run.TotalCases = run.Results.Count;
        run.PassedCases = run.Results.Count(r => r.Outcome == TestResultOutcome.Pass);
        run.FailedCases = run.Results.Count(r => r.Outcome == TestResultOutcome.Fail);
        run.BlockedCases = run.Results.Count(r => r.Outcome == TestResultOutcome.Blocked);
    }

    private static PrototypeTestRunStatus DeriveRunStatus(PrototypeTestRun run)
    {
        if (run.Results.Count == 0)
        {
            return PrototypeTestRunStatus.Planned;
        }

        if (run.FailedCases > 0)
        {
            return PrototypeTestRunStatus.Failed;
        }

        if (run.BlockedCases > 0)
        {
            return PrototypeTestRunStatus.Blocked;
        }

        return run.PassedCases == run.TotalCases ? PrototypeTestRunStatus.Passed : PrototypeTestRunStatus.InProgress;
    }

    private async Task ValidateLinksAsync(
        Guid userId, Guid? projectId, Guid? strategyId, Guid? experimentId, CancellationToken cancellationToken)
    {
        if (projectId.HasValue && !await _links.IsResearchProjectMemberAsync(projectId.Value, userId, cancellationToken))
        {
            throw new ConflictException("You can only link a research project you belong to.");
        }

        if (strategyId.HasValue && !await _links.PreservationStrategyOwnedByAsync(strategyId.Value, userId, cancellationToken))
        {
            throw new ConflictException("You can only link a preservation strategy you own.");
        }

        if (experimentId.HasValue && !await _links.InnovationExperimentOwnedByAsync(experimentId.Value, userId, cancellationToken))
        {
            throw new ConflictException("You can only link an experiment you own.");
        }
    }

    private static TestCasePriority? ParsePriority(string? value)
        => Enum.TryParse<TestCasePriority>(value, true, out var parsed) ? parsed : null;

    private static PrototypeIssueSeverity? ParseSeverity(string? value)
        => Enum.TryParse<PrototypeIssueSeverity>(value, true, out var parsed) ? parsed : null;
}
