using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Security;

namespace ShilpoHubBD.Application.Services.Security;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;

    public AuditLogService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task LogAsync(
        Guid? actorUserId, string actorName, string action, string entityType, Guid? entityId,
        string description, string? ipAddress, CancellationToken cancellationToken)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            ActorName = actorName,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow,
        };

        await _repository.AddAsync(log, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<AuditLogDto>> GetPagedAsync(AuditLogQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _repository.GetPagedAsync(query, cancellationToken);
        return new PagedResult<AuditLogDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    private static AuditLogDto ToDto(AuditLog log) => new()
    {
        Id = log.Id,
        ActorUserId = log.ActorUserId,
        ActorName = log.ActorName,
        Action = log.Action,
        EntityType = log.EntityType,
        EntityId = log.EntityId,
        Description = log.Description,
        IpAddress = log.IpAddress,
        CreatedAt = log.CreatedAt,
    };
}
