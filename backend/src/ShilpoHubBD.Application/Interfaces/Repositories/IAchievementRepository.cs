using ShilpoHubBD.Domain.Entities.Achievement;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IAchievementRepository
{
    Task<int> GetTotalXpAsync(Guid userId, CancellationToken cancellationToken);
    Task<List<XpTransaction>> GetXpHistoryAsync(Guid userId, CancellationToken cancellationToken);
    Task AddXpTransactionAsync(XpTransaction transaction, CancellationToken cancellationToken);

    Task<List<Achievement>> GetAllAchievementsAsync(CancellationToken cancellationToken);
    Task<Achievement?> GetAchievementByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAchievementAsync(Achievement achievement, CancellationToken cancellationToken);

    Task<List<UserAchievement>> GetUserAchievementsAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> HasUserAchievementAsync(Guid userId, Guid achievementId, CancellationToken cancellationToken);
    Task AddUserAchievementAsync(UserAchievement userAchievement, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
