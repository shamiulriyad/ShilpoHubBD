using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Admin;

namespace ShilpoHubBD.Data.Repositories;

public class IdentityVerificationRepository : IIdentityVerificationRepository
{
    private readonly ShilpoHubDbContext _context;

    public IdentityVerificationRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(IdentityVerificationRequest request, CancellationToken cancellationToken)
        => await _context.IdentityVerificationRequests.AddAsync(request, cancellationToken);

    public Task<IdentityVerificationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.IdentityVerificationRequests
            .Include(r => r.User)
            .Include(r => r.ReviewedBy)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<(List<IdentityVerificationRequest> Items, int TotalCount)> GetPagedAsync(
        IdentityVerificationQueryParameters query, CancellationToken cancellationToken)
    {
        var requests = _context.IdentityVerificationRequests
            .Include(r => r.User)
            .Include(r => r.ReviewedBy)
            .AsQueryable();

        if (TryEnum<IdentityVerificationStatus>(query.Status, out var status))
        {
            requests = requests.Where(r => r.Status == status);
        }

        if (TryEnum<IdentityVerificationType>(query.Type, out var type))
        {
            requests = requests.Where(r => r.Type == type);
        }

        if (query.UserId.HasValue)
        {
            requests = requests.Where(r => r.UserId == query.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            requests = requests.Where(r =>
                r.User.FullName.ToLower().Contains(term)
                || r.User.Email.ToLower().Contains(term)
                || r.DocumentNumber.ToLower().Contains(term));
        }

        requests = requests
            .OrderByDescending(r => r.Status == IdentityVerificationStatus.Pending)
            .ThenByDescending(r => r.SubmittedAt);

        var totalCount = await requests.CountAsync(cancellationToken);
        var items = await requests
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<List<IdentityVerificationRequest>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        => _context.IdentityVerificationRequests
            .Include(r => r.ReviewedBy)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync(cancellationToken);

    public async Task<Dictionary<Guid, IdentityVerificationStatus>> GetLatestStatusesByUserIdsAsync(
        IEnumerable<Guid> userIds, CancellationToken cancellationToken)
    {
        var ids = userIds.ToList();
        var requests = await _context.IdentityVerificationRequests
            .Where(r => ids.Contains(r.UserId))
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync(cancellationToken);

        return requests
            .GroupBy(r => r.UserId)
            .ToDictionary(g => g.Key, g => g.First().Status);
    }

    public Task<bool> HasPendingRequestAsync(Guid userId, CancellationToken cancellationToken)
        => _context.IdentityVerificationRequests
            .AnyAsync(r => r.UserId == userId && r.Status == IdentityVerificationStatus.Pending, cancellationToken);

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
