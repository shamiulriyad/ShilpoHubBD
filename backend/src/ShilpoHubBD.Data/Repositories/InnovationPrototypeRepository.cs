using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Data.Repositories;

public class InnovationPrototypeRepository : IInnovationPrototypeRepository
{
    private readonly ShilpoHubDbContext _context;

    public InnovationPrototypeRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<InnovationPrototype?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.InnovationPrototypes
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<InnovationPrototype?> GetDetailAsync(Guid id, CancellationToken cancellationToken)
        => _context.InnovationPrototypes
            .Include(p => p.Owner)
            .Include(p => p.Iterations).ThenInclude(i => i.CreatedBy)
            .Include(p => p.TestCases)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<(List<InnovationPrototype> Items, int TotalCount)> GetPagedForOwnerAsync(
        Guid ownerUserId, InnovationPrototypeQueryParameters query, CancellationToken cancellationToken)
    {
        var prototypes = _context.InnovationPrototypes
            .Include(p => p.Owner)
            .Include(p => p.Iterations)
            .Include(p => p.TestCases)
            .Include(p => p.TestRuns)
            .Include(p => p.Issues)
            .AsSplitQuery()
            .Where(p => p.OwnerUserId == ownerUserId);

        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<InnovationPrototypeStatus>(query.Status, true, out var status))
        {
            prototypes = prototypes.Where(p => p.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            prototypes = prototypes.Where(p => p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term));
        }

        prototypes = prototypes.OrderByDescending(p => p.UpdatedAt);

        var totalCount = await prototypes.CountAsync(cancellationToken);
        var items = await prototypes
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(InnovationPrototype prototype, CancellationToken cancellationToken)
        => await _context.InnovationPrototypes.AddAsync(prototype, cancellationToken);

    public void Remove(InnovationPrototype prototype)
        => _context.InnovationPrototypes.Remove(prototype);

    // ---- iterations ----

    public Task<PrototypeIteration?> GetIterationByIdAsync(Guid iterationId, CancellationToken cancellationToken)
        => _context.PrototypeIterations
            .Include(i => i.CreatedBy)
            .FirstOrDefaultAsync(i => i.Id == iterationId, cancellationToken);

    public async Task<int> GetMaxIterationNumberAsync(Guid prototypeId, CancellationToken cancellationToken)
        => (await _context.PrototypeIterations
            .Where(i => i.InnovationPrototypeId == prototypeId)
            .Select(i => (int?)i.VersionNumber)
            .MaxAsync(cancellationToken)) ?? 0;

    public Task<List<PrototypeIteration>> GetIterationsAsync(Guid prototypeId, CancellationToken cancellationToken)
        => _context.PrototypeIterations
            .Include(i => i.CreatedBy)
            .Where(i => i.InnovationPrototypeId == prototypeId)
            .OrderByDescending(i => i.VersionNumber)
            .ToListAsync(cancellationToken);

    public async Task AddIterationAsync(PrototypeIteration iteration, CancellationToken cancellationToken)
        => await _context.PrototypeIterations.AddAsync(iteration, cancellationToken);

    // ---- test cases ----

    public Task<PrototypeTestCase?> GetTestCaseByIdAsync(Guid testCaseId, CancellationToken cancellationToken)
        => _context.PrototypeTestCases.FirstOrDefaultAsync(c => c.Id == testCaseId, cancellationToken);

    public Task<List<PrototypeTestCase>> GetTestCasesAsync(Guid prototypeId, CancellationToken cancellationToken)
        => _context.PrototypeTestCases
            .Where(c => c.InnovationPrototypeId == prototypeId)
            .OrderBy(c => c.OrderIndex).ThenBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddTestCaseAsync(PrototypeTestCase testCase, CancellationToken cancellationToken)
        => await _context.PrototypeTestCases.AddAsync(testCase, cancellationToken);

    public void RemoveTestCase(PrototypeTestCase testCase)
        => _context.PrototypeTestCases.Remove(testCase);

    // ---- test runs ----

    public Task<PrototypeTestRun?> GetTestRunByIdAsync(Guid testRunId, CancellationToken cancellationToken)
        => _context.PrototypeTestRuns
            .Include(r => r.ExecutedBy)
            .Include(r => r.Iteration)
            .Include(r => r.Results)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == testRunId, cancellationToken);

    public async Task<int> GetMaxTestRunNumberAsync(Guid prototypeId, CancellationToken cancellationToken)
        => (await _context.PrototypeTestRuns
            .Where(r => r.InnovationPrototypeId == prototypeId)
            .Select(r => (int?)r.RunNumber)
            .MaxAsync(cancellationToken)) ?? 0;

    public async Task<(List<PrototypeTestRun> Items, int TotalCount)> GetTestRunsAsync(
        Guid prototypeId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var runs = _context.PrototypeTestRuns
            .Include(r => r.ExecutedBy)
            .Include(r => r.Iteration)
            .Include(r => r.Results)
            .AsSplitQuery()
            .Where(r => r.InnovationPrototypeId == prototypeId)
            .OrderByDescending(r => r.RunNumber);

        var totalCount = await runs.CountAsync(cancellationToken);
        var items = await runs.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task AddTestRunAsync(PrototypeTestRun testRun, CancellationToken cancellationToken)
        => await _context.PrototypeTestRuns.AddAsync(testRun, cancellationToken);

    public void RemoveTestRun(PrototypeTestRun testRun)
        => _context.PrototypeTestRuns.Remove(testRun);

    public void RemoveTestResults(IEnumerable<PrototypeTestResult> results)
        => _context.PrototypeTestResults.RemoveRange(results);

    // ---- issues ----

    public Task<PrototypeIssue?> GetIssueByIdAsync(Guid issueId, CancellationToken cancellationToken)
        => _context.PrototypeIssues
            .Include(i => i.ReportedBy)
            .Include(i => i.ResolvedBy)
            .FirstOrDefaultAsync(i => i.Id == issueId, cancellationToken);

    public async Task<(List<PrototypeIssue> Items, int TotalCount)> GetIssuesAsync(
        Guid prototypeId, string? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        var issues = _context.PrototypeIssues
            .Include(i => i.ReportedBy)
            .Include(i => i.ResolvedBy)
            .Where(i => i.InnovationPrototypeId == prototypeId);

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<PrototypeIssueStatus>(status, true, out var parsed))
        {
            issues = issues.Where(i => i.Status == parsed);
        }

        issues = issues.OrderByDescending(i => i.CreatedAt);

        var totalCount = await issues.CountAsync(cancellationToken);
        var items = await issues.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public Task<int> CountOpenIssuesAsync(Guid prototypeId, CancellationToken cancellationToken)
        => _context.PrototypeIssues.CountAsync(
            i => i.InnovationPrototypeId == prototypeId
                && i.Status != PrototypeIssueStatus.Resolved
                && i.Status != PrototypeIssueStatus.Closed
                && i.Status != PrototypeIssueStatus.WontFix,
            cancellationToken);

    public async Task AddIssueAsync(PrototypeIssue issue, CancellationToken cancellationToken)
        => await _context.PrototypeIssues.AddAsync(issue, cancellationToken);

    public void RemoveIssue(PrototypeIssue issue)
        => _context.PrototypeIssues.Remove(issue);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
