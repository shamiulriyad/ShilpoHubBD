using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IComplianceService
{
    Task<ComplianceRecordDto> CreateAsync(
        Guid userId, CreateComplianceRecordRequest request, CancellationToken cancellationToken);

    Task<PagedResult<ComplianceRecordListItemDto>> GetPagedAsync(
        ComplianceQueryParameters query, CancellationToken cancellationToken);

    Task<ComplianceRecordDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ComplianceRecordDto> UpdateAsync(
        Guid userId, Guid id, UpdateComplianceRecordRequest request, CancellationToken cancellationToken);

    Task<ComplianceRecordDto> UpsertRequirementAsync(
        Guid userId, Guid id, UpsertComplianceRequirementRequest request, CancellationToken cancellationToken);

    Task<ComplianceRecordDto> RemoveRequirementAsync(
        Guid userId, Guid id, Guid requirementId, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
