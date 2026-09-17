using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Security;

namespace ShilpoHubBD.Data.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ShilpoHubDbContext _context;

    public AuditLogRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog log, CancellationToken cancellationToken)
        => await _context.AuditLogs.AddAsync(log, cancellationToken);

    public async Task<(List<AuditLog> Items, int TotalCount)> GetPagedAsync(
        AuditLogQueryParameters query, CancellationToken cancellationToken)
    {
        var logs = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            logs = logs.Where(a => a.Action == query.Action);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            logs = logs.Where(a => a.EntityType == query.EntityType);
        }

        if (query.ActorUserId.HasValue)
        {
            logs = logs.Where(a => a.ActorUserId == query.ActorUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            logs = logs.Where(a => EF.Functions.ILike(a.Description, term) || EF.Functions.ILike(a.ActorName, term));
        }

        if (query.From.HasValue)
        {
            logs = logs.Where(a => a.CreatedAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            logs = logs.Where(a => a.CreatedAt <= query.To.Value);
        }

        logs = logs.OrderByDescending(a => a.CreatedAt);

        var totalCount = await logs.CountAsync(cancellationToken);
        var items = await logs
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
