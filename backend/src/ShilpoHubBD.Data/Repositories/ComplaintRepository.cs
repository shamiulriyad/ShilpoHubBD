using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Data.Repositories;

public class ComplaintRepository : IComplaintRepository
{
    private readonly ShilpoHubDbContext _context;

    public ComplaintRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Complaint complaint, CancellationToken cancellationToken)
        => await _context.Complaints.AddAsync(complaint, cancellationToken);

    public void Remove(Complaint complaint) => _context.Complaints.Remove(complaint);

    public Task<Complaint?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.Complaints
            .Include(c => c.ComplainantUser)
            .Include(c => c.AssignedTo)
            .Include(c => c.ResolvedBy)
            .Include(c => c.Updates).ThenInclude(u => u.Actor)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<(List<Complaint> Items, int TotalCount)> GetPagedAsync(
        ComplaintQueryParameters query, CancellationToken cancellationToken)
    {
        var complaints = _context.Complaints
            .Include(c => c.AssignedTo)
            .AsQueryable();

        if (TryEnum<ComplaintCategory>(query.Category, out var category))
        {
            complaints = complaints.Where(c => c.Category == category);
        }

        if (TryEnum<ComplaintStatus>(query.Status, out var status))
        {
            complaints = complaints.Where(c => c.Status == status);
        }

        if (TryEnum<ComplaintPriority>(query.Priority, out var priority))
        {
            complaints = complaints.Where(c => c.Priority == priority);
        }

        if (query.AssignedToUserId.HasValue)
        {
            complaints = complaints.Where(c => c.AssignedToUserId == query.AssignedToUserId.Value);
        }

        if (query.ComplainantUserId.HasValue)
        {
            complaints = complaints.Where(c => c.ComplainantUserId == query.ComplainantUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            complaints = complaints.Where(c =>
                c.Title.ToLower().Contains(term)
                || c.ReferenceCode.ToLower().Contains(term)
                || (c.AgainstLabel != null && c.AgainstLabel.ToLower().Contains(term)));
        }

        complaints = complaints
            .OrderByDescending(c => c.Status == ComplaintStatus.Submitted || c.Status == ComplaintStatus.Triaged
                || c.Status == ComplaintStatus.InProgress)
            .ThenByDescending(c => c.Priority)
            .ThenByDescending(c => c.CreatedAt);

        var totalCount = await complaints.CountAsync(cancellationToken);
        var items = await complaints
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<bool> ReferenceExistsAsync(string referenceCode, CancellationToken cancellationToken)
        => _context.Complaints.AnyAsync(c => c.ReferenceCode == referenceCode, cancellationToken);

    public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
        => _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);

    public Task<bool> OrderExistsAsync(Guid orderId, CancellationToken cancellationToken)
        => _context.Orders.AnyAsync(o => o.Id == orderId, cancellationToken);

    public Task<bool> FlagExistsAsync(Guid flagId, CancellationToken cancellationToken)
        => _context.MonitoringFlags.AnyAsync(f => f.Id == flagId, cancellationToken);

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
