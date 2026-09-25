using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Profiles;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Data.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly ShilpoHubDbContext _context;

    public UserProfileRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<UserProfile> WithDetails()
        => _context.UserProfiles.Include(p => p.User).Include(p => p.District);

    public Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    public Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> NidInUseAsync(string nidNumber, Guid exceptUserId, CancellationToken cancellationToken)
        => _context.UserProfiles.AnyAsync(p => p.NidNumber == nidNumber && p.UserId != exceptUserId, cancellationToken);

    public async Task<List<string>> GetProducerExpertiseOptionsAsync(CancellationToken cancellationToken)
    {
        var values = await _context.UserProfiles
            .Where(p => p.Status == UserProfileStatus.Approved && p.Expertise != null
                && _context.UserRoles.Any(r => r.UserId == p.UserId && r.Role.Name == "Producer"))
            .Select(p => p.Expertise!)
            .ToListAsync(cancellationToken);
        return values.Select(v => v.Trim()).Where(v => v.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(v => v).ToList();
    }

    public Task<bool> IsApprovedAsync(Guid userId, CancellationToken cancellationToken)
        => _context.UserProfiles.AnyAsync(p => p.UserId == userId && p.Status == UserProfileStatus.Approved, cancellationToken);

    public async Task<(List<UserProfile> Items, int TotalCount)> GetPagedAsync(UserProfileQueryParameters query, CancellationToken cancellationToken)
    {
        var profiles = WithDetails().AsQueryable();

        if (query.Status.HasValue)
        {
            profiles = profiles.Where(p => p.Status == query.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            profiles = profiles.Where(p => EF.Functions.ILike(p.LegalName, term) || EF.Functions.ILike(p.NidNumber, term)
                || EF.Functions.ILike(p.User.Email, term));
        }

        profiles = profiles.OrderBy(p => p.Status).ThenByDescending(p => p.UpdatedAt);

        var total = await profiles.CountAsync(cancellationToken);
        var items = await profiles.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<Dictionary<Guid, List<string>>> GetRolesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken)
    {
        var ids = userIds.ToList();
        var rows = await _context.UserRoles.Where(r => ids.Contains(r.UserId)).Select(r => new { r.UserId, r.Role.Name }).ToListAsync(cancellationToken);
        return rows.GroupBy(r => r.UserId).ToDictionary(g => g.Key, g => g.Select(r => r.Name).ToList());
    }

    public Task<bool> DistrictExistsAsync(Guid districtId, CancellationToken cancellationToken)
        => _context.Districts.AnyAsync(d => d.Id == districtId, cancellationToken);

    public async Task AddAsync(UserProfile profile, CancellationToken cancellationToken)
        => await _context.UserProfiles.AddAsync(profile, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
