using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Innovation;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IInnovationPrototypeService
{
    Task<PagedResult<InnovationPrototypeListItemDto>> GetMineAsync(
        Guid userId, InnovationPrototypeQueryParameters query, CancellationToken cancellationToken);

    Task<InnovationPrototypeDetailDto> GetByIdAsync(Guid userId, Guid prototypeId, CancellationToken cancellationToken);

    Task<InnovationPrototypeDetailDto> CreateAsync(
        Guid userId, bool isResearcher, CreateInnovationPrototypeRequest request, CancellationToken cancellationToken);

    Task<InnovationPrototypeDetailDto> UpdateAsync(
        Guid userId, bool isResearcher, Guid prototypeId, UpdateInnovationPrototypeRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid prototypeId, CancellationToken cancellationToken);

    Task<PrototypeIterationDto> AddIterationAsync(
        Guid userId, Guid prototypeId, CreatePrototypeIterationRequest request, CancellationToken cancellationToken);

    Task<PrototypeTestCaseDto> AddTestCaseAsync(
        Guid userId, Guid prototypeId, CreatePrototypeTestCaseRequest request, CancellationToken cancellationToken);
    Task<PrototypeTestCaseDto> UpdateTestCaseAsync(
        Guid userId, Guid prototypeId, Guid testCaseId, UpdatePrototypeTestCaseRequest request, CancellationToken cancellationToken);
    Task DeleteTestCaseAsync(Guid userId, Guid prototypeId, Guid testCaseId, CancellationToken cancellationToken);

    Task<PagedResult<PrototypeTestRunDto>> GetTestRunsAsync(
        Guid userId, Guid prototypeId, int page, int pageSize, CancellationToken cancellationToken);
    Task<PrototypeTestRunDto> GetTestRunAsync(Guid userId, Guid prototypeId, Guid testRunId, CancellationToken cancellationToken);
    Task<PrototypeTestRunDto> CreateTestRunAsync(
        Guid userId, Guid prototypeId, CreatePrototypeTestRunRequest request, CancellationToken cancellationToken);
    Task<PrototypeTestRunDto> UpdateTestRunAsync(
        Guid userId, Guid prototypeId, Guid testRunId, UpdatePrototypeTestRunRequest request, CancellationToken cancellationToken);
    Task DeleteTestRunAsync(Guid userId, Guid prototypeId, Guid testRunId, CancellationToken cancellationToken);

    Task<PagedResult<PrototypeIssueDto>> GetIssuesAsync(
        Guid userId, Guid prototypeId, string? status, int page, int pageSize, CancellationToken cancellationToken);
    Task<PrototypeIssueDto> CreateIssueAsync(
        Guid userId, Guid prototypeId, CreatePrototypeIssueRequest request, CancellationToken cancellationToken);
    Task<PrototypeIssueDto> UpdateIssueAsync(
        Guid userId, Guid prototypeId, Guid issueId, UpdatePrototypeIssueRequest request, CancellationToken cancellationToken);
    Task DeleteIssueAsync(Guid userId, Guid prototypeId, Guid issueId, CancellationToken cancellationToken);
}
