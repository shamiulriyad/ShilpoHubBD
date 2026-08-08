using ShilpoHubBD.Domain.Entities.Passport;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IPassportRepository
{
    Task<List<Badge>> GetAllBadgesAsync(CancellationToken cancellationToken);
    Task<Badge?> GetBadgeByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Badge?> GetDistrictBadgeAsync(Guid districtId, CancellationToken cancellationToken);
    Task<List<Badge>> GetBadgesByTypeAsync(BadgeType type, CancellationToken cancellationToken);
    Task AddBadgeAsync(Badge badge, CancellationToken cancellationToken);
    Task<List<UserBadge>> GetUserBadgesAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> HasUserBadgeAsync(Guid userId, Guid badgeId, CancellationToken cancellationToken);
    Task AddUserBadgeAsync(UserBadge userBadge, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
