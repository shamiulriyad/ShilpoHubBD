using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Passport;

namespace ShilpoHubBD.Data.Repositories;

public class PassportRepository : IPassportRepository
{
    private readonly ShilpoHubDbContext _context;

    public PassportRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<Badge> WithDetails()
        => _context.Badges.Include(b => b.District);

    public Task<List<Badge>> GetAllBadgesAsync(CancellationToken cancellationToken)
        => WithDetails().OrderBy(b => b.Type).ThenBy(b => b.Name).ToListAsync(cancellationToken);

    public Task<Badge?> GetBadgeByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<Badge?> GetDistrictBadgeAsync(Guid districtId, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(b => b.Type == BadgeType.District && b.DistrictId == districtId, cancellationToken);

    public Task<List<Badge>> GetBadgesByTypeAsync(BadgeType type, CancellationToken cancellationToken)
        => WithDetails().Where(b => b.Type == type).ToListAsync(cancellationToken);

    public async Task AddBadgeAsync(Badge badge, CancellationToken cancellationToken)
        => await _context.Badges.AddAsync(badge, cancellationToken);

    public Task<List<UserBadge>> GetUserBadgesAsync(Guid userId, CancellationToken cancellationToken)
        => _context.UserBadges
            .Include(ub => ub.Badge)
            .Where(ub => ub.UserId == userId)
            .OrderByDescending(ub => ub.EarnedAt)
            .ToListAsync(cancellationToken);

    public Task<bool> HasUserBadgeAsync(Guid userId, Guid badgeId, CancellationToken cancellationToken)
        => _context.UserBadges.AnyAsync(ub => ub.UserId == userId && ub.BadgeId == badgeId, cancellationToken);

    public async Task AddUserBadgeAsync(UserBadge userBadge, CancellationToken cancellationToken)
        => await _context.UserBadges.AddAsync(userBadge, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
