using ShilpoHubBD.Application.DTOs.Achievement;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IAchievementService
{
    Task<XpSummaryDto> GetMyXpSummaryAsync(Guid userId, CancellationToken cancellationToken);
    Task<List<XpTransactionDto>> GetMyXpHistoryAsync(Guid userId, CancellationToken cancellationToken);

    Task<List<AchievementDto>> GetAllAchievementsAsync(CancellationToken cancellationToken);
    Task<List<UserAchievementDto>> GetMyAchievementsAsync(Guid userId, CancellationToken cancellationToken);
    Task<AchievementDto> CreateAchievementAsync(CreateAchievementRequest request, CancellationToken cancellationToken);

    Task<XpSummaryDto> AwardXpAsync(AwardXpRequest request, CancellationToken cancellationToken);
    Task<List<UserAchievementDto>> EvaluateAchievementsAsync(Guid userId, CancellationToken cancellationToken);
}
