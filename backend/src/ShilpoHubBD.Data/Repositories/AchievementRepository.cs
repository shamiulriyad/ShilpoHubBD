using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Achievement;

namespace ShilpoHubBD.Data.Repositories;

public class AchievementRepository : IAchievementRepository
{
    private readonly ShilpoHubDbContext _context;

    public AchievementRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetTotalXpAsync(Guid userId, CancellationToken cancellationToken)
    {
        var total = await _context.XpTransactions
            .Where(t => t.UserId == userId)
            .SumAsync(t => (int?)t.Amount, cancellationToken);

        return total ?? 0;
    }

    public Task<List<XpTransaction>> GetXpHistoryAsync(Guid userId, CancellationToken cancellationToken)
        => _context.XpTransactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddXpTransactionAsync(XpTransaction transaction, CancellationToken cancellationToken)
        => await _context.XpTransactions.AddAsync(transaction, cancellationToken);

    public Task<List<Achievement>> GetAllAchievementsAsync(CancellationToken cancellationToken)
        => _context.Achievements.OrderBy(a => a.RequiredXp).ToListAsync(cancellationToken);

    public Task<Achievement?> GetAchievementByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.Achievements.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task AddAchievementAsync(Achievement achievement, CancellationToken cancellationToken)
        => await _context.Achievements.AddAsync(achievement, cancellationToken);

    public Task<List<UserAchievement>> GetUserAchievementsAsync(Guid userId, CancellationToken cancellationToken)
        => _context.UserAchievements
            .Include(ua => ua.Achievement)
            .Where(ua => ua.UserId == userId)
            .OrderByDescending(ua => ua.UnlockedAt)
            .ToListAsync(cancellationToken);

    public Task<bool> HasUserAchievementAsync(Guid userId, Guid achievementId, CancellationToken cancellationToken)
        => _context.UserAchievements.AnyAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId, cancellationToken);

    public async Task AddUserAchievementAsync(UserAchievement userAchievement, CancellationToken cancellationToken)
        => await _context.UserAchievements.AddAsync(userAchievement, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
