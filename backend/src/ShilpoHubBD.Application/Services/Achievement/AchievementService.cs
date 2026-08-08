using ShilpoHubBD.Application.DTOs.Achievement;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Achievement;

namespace ShilpoHubBD.Application.Services.Achievement;

public class AchievementService : IAchievementService
{
    private const int XpPerLevel = 500;

    private readonly IAchievementRepository _achievementRepository;
    private readonly IUserRepository _userRepository;

    public AchievementService(IAchievementRepository achievementRepository, IUserRepository userRepository)
    {
        _achievementRepository = achievementRepository;
        _userRepository = userRepository;
    }

    public async Task<XpSummaryDto> GetMyXpSummaryAsync(Guid userId, CancellationToken cancellationToken)
    {
        var totalXp = await _achievementRepository.GetTotalXpAsync(userId, cancellationToken);
        return BuildXpSummary(totalXp);
    }

    public async Task<List<XpTransactionDto>> GetMyXpHistoryAsync(Guid userId, CancellationToken cancellationToken)
    {
        var history = await _achievementRepository.GetXpHistoryAsync(userId, cancellationToken);
        return history.Select(ToXpTransactionDto).ToList();
    }

    public async Task<List<AchievementDto>> GetAllAchievementsAsync(CancellationToken cancellationToken)
    {
        var achievements = await _achievementRepository.GetAllAchievementsAsync(cancellationToken);
        return achievements.Select(ToAchievementDto).ToList();
    }

    public async Task<List<UserAchievementDto>> GetMyAchievementsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userAchievements = await _achievementRepository.GetUserAchievementsAsync(userId, cancellationToken);
        return userAchievements.Select(ToUserAchievementDto).ToList();
    }

    public async Task<AchievementDto> CreateAchievementAsync(CreateAchievementRequest request, CancellationToken cancellationToken)
    {
        var achievement = new Domain.Entities.Achievement.Achievement
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            IconUrl = request.IconUrl?.Trim(),
            RequiredXp = request.RequiredXp,
            XpReward = request.XpReward,
            CreatedAt = DateTime.UtcNow,
        };

        await _achievementRepository.AddAchievementAsync(achievement, cancellationToken);
        await _achievementRepository.SaveChangesAsync(cancellationToken);

        return ToAchievementDto(achievement);
    }

    public async Task<XpSummaryDto> AwardXpAsync(AwardXpRequest request, CancellationToken cancellationToken)
    {
        if (await _userRepository.GetByIdAsync(request.UserId, cancellationToken) is null)
        {
            throw new NotFoundException("User not found.");
        }

        var transaction = new XpTransaction
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Amount = request.Amount,
            Reason = request.Reason.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        await _achievementRepository.AddXpTransactionAsync(transaction, cancellationToken);
        await _achievementRepository.SaveChangesAsync(cancellationToken);

        await EvaluateAchievementsAsync(request.UserId, cancellationToken);

        var totalXp = await _achievementRepository.GetTotalXpAsync(request.UserId, cancellationToken);
        return BuildXpSummary(totalXp);
    }

    public async Task<List<UserAchievementDto>> EvaluateAchievementsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var totalXp = await _achievementRepository.GetTotalXpAsync(userId, cancellationToken);
        var achievements = await _achievementRepository.GetAllAchievementsAsync(cancellationToken);

        var newlyUnlocked = new List<UserAchievementDto>();

        foreach (var achievement in achievements.Where(a => a.RequiredXp <= totalXp))
        {
            if (await _achievementRepository.HasUserAchievementAsync(userId, achievement.Id, cancellationToken))
            {
                continue;
            }

            var now = DateTime.UtcNow;
            var userAchievement = new UserAchievement
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AchievementId = achievement.Id,
                UnlockedAt = now,
            };

            await _achievementRepository.AddUserAchievementAsync(userAchievement, cancellationToken);

            if (achievement.XpReward > 0)
            {
                await _achievementRepository.AddXpTransactionAsync(new XpTransaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Amount = achievement.XpReward,
                    Reason = $"Achievement unlocked: {achievement.Name}",
                    CreatedAt = now,
                }, cancellationToken);
            }

            userAchievement.Achievement = achievement;
            newlyUnlocked.Add(ToUserAchievementDto(userAchievement));
        }

        if (newlyUnlocked.Count > 0)
        {
            await _achievementRepository.SaveChangesAsync(cancellationToken);
        }

        return newlyUnlocked;
    }

    private static XpSummaryDto BuildXpSummary(int totalXp)
    {
        var xpIntoLevel = totalXp % XpPerLevel;

        return new XpSummaryDto
        {
            TotalXp = totalXp,
            Level = 1 + totalXp / XpPerLevel,
            XpIntoCurrentLevel = xpIntoLevel,
            XpForNextLevel = XpPerLevel,
            XpToNextLevel = XpPerLevel - xpIntoLevel,
        };
    }

    private static XpTransactionDto ToXpTransactionDto(XpTransaction transaction) => new()
    {
        Id = transaction.Id,
        Amount = transaction.Amount,
        Reason = transaction.Reason,
        CreatedAt = transaction.CreatedAt,
    };

    private static AchievementDto ToAchievementDto(Domain.Entities.Achievement.Achievement achievement) => new()
    {
        Id = achievement.Id,
        Name = achievement.Name,
        Description = achievement.Description,
        IconUrl = achievement.IconUrl,
        RequiredXp = achievement.RequiredXp,
        XpReward = achievement.XpReward,
        CreatedAt = achievement.CreatedAt,
    };

    private static UserAchievementDto ToUserAchievementDto(UserAchievement userAchievement) => new()
    {
        Id = userAchievement.Id,
        AchievementId = userAchievement.AchievementId,
        AchievementName = userAchievement.Achievement.Name,
        IconUrl = userAchievement.Achievement.IconUrl,
        UnlockedAt = userAchievement.UnlockedAt,
    };
}
