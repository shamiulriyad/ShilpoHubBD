using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Domain.Entities.Security;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken cancellationToken);
    Task<(List<AuditLog> Items, int TotalCount)> GetPagedAsync(AuditLogQueryParameters query, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
