using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IComplianceRepository
{
    Task AddAsync(ComplianceRecord record, CancellationToken cancellationToken);

    void Remove(ComplianceRecord record);

    Task<ComplianceRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<(List<ComplianceRecord> Items, int TotalCount)> GetPagedAsync(
        ComplianceQueryParameters query, CancellationToken cancellationToken);

    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
