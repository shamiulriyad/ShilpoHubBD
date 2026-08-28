using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Data.Repositories;

public class ComplianceRepository : IComplianceRepository
{
    private readonly ShilpoHubDbContext _context;

    public ComplianceRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ComplianceRecord record, CancellationToken cancellationToken)
        => await _context.ComplianceRecords.AddAsync(record, cancellationToken);

    public void Remove(ComplianceRecord record) => _context.ComplianceRecords.Remove(record);

    public Task<ComplianceRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.ComplianceRecords
            .Include(r => r.CreatedBy)
            .Include(r => r.Reviewer)
            .Include(r => r.Requirements)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<(List<ComplianceRecord> Items, int TotalCount)> GetPagedAsync(
        ComplianceQueryParameters query, CancellationToken cancellationToken)
    {
        var records = _context.ComplianceRecords.AsQueryable();

        if (TryEnum<ComplianceEntityType>(query.EntityType, out var entityType))
        {
            records = records.Where(r => r.EntityType == entityType);
        }

        if (query.EntityId.HasValue)
        {
            records = records.Where(r => r.EntityId == query.EntityId.Value);
        }

        if (TryEnum<ComplianceStatus>(query.Status, out var status))
        {
            records = records.Where(r => r.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Framework))
        {
            var fw = query.Framework.Trim().ToLower();
            records = records.Where(r => r.Framework.ToLower().Contains(fw));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            records = records.Where(r => r.EntityLabel.ToLower().Contains(term)
                || r.Framework.ToLower().Contains(term));
        }

        if (query.ReviewDueOnly == true)
        {
            var now = DateTime.UtcNow;
            records = records.Where(r => r.NextReviewDue != null && r.NextReviewDue <= now);
        }

        records = records
            .OrderBy(r => r.NextReviewDue ?? DateTime.MaxValue)
            .ThenByDescending(r => r.UpdatedAt);

        var totalCount = await records.CountAsync(cancellationToken);
        var items = await records
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
        => _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);

    private static bool TryEnum<T>(string? value, out T result) where T : struct, Enum
    {
        if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse(value, true, out result))
        {
            return true;
        }

        result = default;
        return false;
    }
}
