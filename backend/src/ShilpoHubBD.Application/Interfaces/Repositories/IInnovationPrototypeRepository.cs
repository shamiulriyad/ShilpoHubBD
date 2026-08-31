using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IInnovationPrototypeRepository
{
    Task<InnovationPrototype?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<InnovationPrototype?> GetDetailAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<InnovationPrototype> Items, int TotalCount)> GetPagedForOwnerAsync(
        Guid ownerUserId, InnovationPrototypeQueryParameters query, CancellationToken cancellationToken);
    Task AddAsync(InnovationPrototype prototype, CancellationToken cancellationToken);
    void Remove(InnovationPrototype prototype);

    Task<PrototypeIteration?> GetIterationByIdAsync(Guid iterationId, CancellationToken cancellationToken);
    Task<int> GetMaxIterationNumberAsync(Guid prototypeId, CancellationToken cancellationToken);
    Task<List<PrototypeIteration>> GetIterationsAsync(Guid prototypeId, CancellationToken cancellationToken);
    Task AddIterationAsync(PrototypeIteration iteration, CancellationToken cancellationToken);

    Task<PrototypeTestCase?> GetTestCaseByIdAsync(Guid testCaseId, CancellationToken cancellationToken);
    Task<List<PrototypeTestCase>> GetTestCasesAsync(Guid prototypeId, CancellationToken cancellationToken);
    Task AddTestCaseAsync(PrototypeTestCase testCase, CancellationToken cancellationToken);
    void RemoveTestCase(PrototypeTestCase testCase);

    Task<PrototypeTestRun?> GetTestRunByIdAsync(Guid testRunId, CancellationToken cancellationToken);
    Task<int> GetMaxTestRunNumberAsync(Guid prototypeId, CancellationToken cancellationToken);
    Task<(List<PrototypeTestRun> Items, int TotalCount)> GetTestRunsAsync(
        Guid prototypeId, int page, int pageSize, CancellationToken cancellationToken);
    Task AddTestRunAsync(PrototypeTestRun testRun, CancellationToken cancellationToken);
    void RemoveTestRun(PrototypeTestRun testRun);
    void RemoveTestResults(IEnumerable<PrototypeTestResult> results);

    Task<PrototypeIssue?> GetIssueByIdAsync(Guid issueId, CancellationToken cancellationToken);
    Task<(List<PrototypeIssue> Items, int TotalCount)> GetIssuesAsync(
        Guid prototypeId, string? status, int page, int pageSize, CancellationToken cancellationToken);
    Task<int> CountOpenIssuesAsync(Guid prototypeId, CancellationToken cancellationToken);
    Task AddIssueAsync(PrototypeIssue issue, CancellationToken cancellationToken);
    void RemoveIssue(PrototypeIssue issue);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
