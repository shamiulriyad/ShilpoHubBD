using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Security;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IAuditLogService
{
    /// <summary>Records a sensitive admin action. Call sites await this alongside their own SaveChanges.</summary>
    Task LogAsync(
        Guid? actorUserId, string actorName, string action, string entityType, Guid? entityId,
        string description, string? ipAddress, CancellationToken cancellationToken);

    Task<PagedResult<AuditLogDto>> GetPagedAsync(AuditLogQueryParameters query, CancellationToken cancellationToken);
}
