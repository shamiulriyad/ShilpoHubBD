using ShilpoHubBD.Application.DTOs.Achievement;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.UnitTests.Common;
using AchievementEntity = ShilpoHubBD.Domain.Entities.Achievement.Achievement;
using AchievementService = ShilpoHubBD.Application.Services.Achievement.AchievementService;
using UserAchievementEntity = ShilpoHubBD.Domain.Entities.Achievement.UserAchievement;
using XpTransactionEntity = ShilpoHubBD.Domain.Entities.Achievement.XpTransaction;

namespace ShilpoHubBD.UnitTests.Features.Achievement.Services;

[Trait("Feature", "Achievement")]
[Trait("Layer", "Service")]
public class AchievementServiceTests
{
    private readonly IAchievementRepository _repository = Substitute.For<IAchievementRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly AchievementService _service;

    public AchievementServiceTests()
    {
        _service = new AchievementService(_repository, _userRepository);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(0, 1, 0, 500)]
    [InlineData(499, 1, 499, 1)]
    [InlineData(500, 2, 0, 500)]
    [InlineData(750, 2, 250, 250)]
    public async Task GetMyXpSummaryAsync_ComputesLevelAndProgressFromTotalXp(int totalXp, int level, int xpIntoLevel, int xpToNext)
    {
        var userId = Guid.NewGuid();
        _repository.GetTotalXpAsync(userId, Arg.Any<CancellationToken>()).Returns(totalXp);

        var result = await _service.GetMyXpSummaryAsync(userId, Ct);

        Assert.Equal(totalXp, result.TotalXp);
        Assert.Equal(level, result.Level);
        Assert.Equal(xpIntoLevel, result.XpIntoCurrentLevel);
        Assert.Equal(500, result.XpForNextLevel);
        Assert.Equal(xpToNext, result.XpToNextLevel);
    }

    [Fact]
    public async Task GetMyXpHistoryAsync_ReturnsMappedTransactions()
    {
        var userId = Guid.NewGuid();
        var transaction = new XpTransactionEntity { Id = Guid.NewGuid(), UserId = userId, Amount = 50, Reason = "Order delivered", CreatedAt = DateTime.UtcNow };
        _repository.GetXpHistoryAsync(userId, Arg.Any<CancellationToken>()).Returns(new List<XpTransactionEntity> { transaction });

        var result = await _service.GetMyXpHistoryAsync(userId, Ct);

        var dto = Assert.Single(result);
        Assert.Equal(transaction.Id, dto.Id);
        Assert.Equal(50, dto.Amount);
        Assert.Equal("Order delivered", dto.Reason);
    }

    [Fact]
    public async Task GetAllAchievementsAsync_ReturnsMappedAchievements()
    {
        var achievement = new AchievementEntity { Id = Guid.NewGuid(), Name = "First Sale", Description = "x", RequiredXp = 100, XpReward = 10, CreatedAt = DateTime.UtcNow };
        _repository.GetAllAchievementsAsync(Arg.Any<CancellationToken>()).Returns(new List<AchievementEntity> { achievement });

        var result = await _service.GetAllAchievementsAsync(Ct);

        Assert.Equal("First Sale", Assert.Single(result).Name);
    }

    [Fact]
    public async Task GetMyAchievementsAsync_ReturnsMappedUserAchievements()
    {
        var userId = Guid.NewGuid();
        var achievement = new AchievementEntity { Id = Guid.NewGuid(), Name = "First Sale", Description = "x", IconUrl = "/icon.png" };
        var userAchievement = new UserAchievementEntity { Id = Guid.NewGuid(), UserId = userId, AchievementId = achievement.Id, Achievement = achievement, UnlockedAt = DateTime.UtcNow };
        _repository.GetUserAchievementsAsync(userId, Arg.Any<CancellationToken>()).Returns(new List<UserAchievementEntity> { userAchievement });

        var result = await _service.GetMyAchievementsAsync(userId, Ct);

        var dto = Assert.Single(result);
        Assert.Equal("First Sale", dto.AchievementName);
        Assert.Equal("/icon.png", dto.IconUrl);
    }

    [Fact]
    public async Task CreateAchievementAsync_TrimsNameDescriptionAndIconUrl_ThenSaves()
    {
        var request = new CreateAchievementRequest { Name = "  First Sale  ", Description = "  Sell your first item  ", IconUrl = "  /icon.png  ", RequiredXp = 100, XpReward = 10 };

        var result = await _service.CreateAchievementAsync(request, Ct);

        await _repository.Received(1).AddAchievementAsync(Arg.Is<AchievementEntity>(a =>
            a.Name == "First Sale" && a.Description == "Sell your first item" && a.IconUrl == "/icon.png"
            && a.RequiredXp == 100 && a.XpReward == 10), Ct);
        await _repository.Received(1).SaveChangesAsync(Ct);
        Assert.Equal("First Sale", result.Name);
    }

    [Fact]
    public async Task CreateAchievementAsync_NullIconUrl_StaysNull()
    {
        var request = new CreateAchievementRequest { Name = "x", Description = "x", IconUrl = null };

        var result = await _service.CreateAchievementAsync(request, Ct);

        Assert.Null(result.IconUrl);
    }

    [Fact]
    public async Task AwardXpAsync_UnknownUser_ThrowsNotFound()
    {
        _userRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ShilpoHubBD.Domain.Entities.Identity.User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.AwardXpAsync(new AwardXpRequest { UserId = Guid.NewGuid(), Amount = 10, Reason = "x" }, Ct));
    }

    [Fact]
    public async Task AwardXpAsync_KnownUser_RecordsTransactionEvaluatesAchievementsAndReturnsNewTotal()
    {
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(TestUsers.Create());
        _repository.GetAllAchievementsAsync(Arg.Any<CancellationToken>()).Returns(new List<AchievementEntity>());
        _repository.GetTotalXpAsync(userId, Arg.Any<CancellationToken>()).Returns(60);

        var result = await _service.AwardXpAsync(new AwardXpRequest { UserId = userId, Amount = 60, Reason = "  Bonus  " }, Ct);

        await _repository.Received(1).AddXpTransactionAsync(Arg.Is<XpTransactionEntity>(t => t.UserId == userId && t.Amount == 60 && t.Reason == "Bonus"), Ct);
        Assert.Equal(60, result.TotalXp);
    }

    [Fact]
    public async Task EvaluateAchievementsAsync_XpBelowThreshold_UnlocksNothing()
    {
        var userId = Guid.NewGuid();
        _repository.GetTotalXpAsync(userId, Arg.Any<CancellationToken>()).Returns(50);
        _repository.GetAllAchievementsAsync(Arg.Any<CancellationToken>()).Returns(new List<AchievementEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "x", RequiredXp = 100 },
        });

        var result = await _service.EvaluateAchievementsAsync(userId, Ct);

        Assert.Empty(result);
        await _repository.DidNotReceive().SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task EvaluateAchievementsAsync_AlreadyUnlocked_IsSkipped()
    {
        var userId = Guid.NewGuid();
        var achievement = new AchievementEntity { Id = Guid.NewGuid(), Name = "x", RequiredXp = 100 };
        _repository.GetTotalXpAsync(userId, Arg.Any<CancellationToken>()).Returns(200);
        _repository.GetAllAchievementsAsync(Arg.Any<CancellationToken>()).Returns(new List<AchievementEntity> { achievement });
        _repository.HasUserAchievementAsync(userId, achievement.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _service.EvaluateAchievementsAsync(userId, Ct);

        Assert.Empty(result);
        await _repository.DidNotReceive().AddUserAchievementAsync(Arg.Any<UserAchievementEntity>(), Ct);
    }

    [Fact]
    public async Task EvaluateAchievementsAsync_NewlyEligible_UnlocksAndAwardsBonusXp()
    {
        var userId = Guid.NewGuid();
        var achievement = new AchievementEntity { Id = Guid.NewGuid(), Name = "Century", RequiredXp = 100, XpReward = 25 };
        _repository.GetTotalXpAsync(userId, Arg.Any<CancellationToken>()).Returns(150);
        _repository.GetAllAchievementsAsync(Arg.Any<CancellationToken>()).Returns(new List<AchievementEntity> { achievement });
        _repository.HasUserAchievementAsync(userId, achievement.Id, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _service.EvaluateAchievementsAsync(userId, Ct);

        var unlocked = Assert.Single(result);
        Assert.Equal("Century", unlocked.AchievementName);
        await _repository.Received(1).AddUserAchievementAsync(Arg.Is<UserAchievementEntity>(ua => ua.UserId == userId && ua.AchievementId == achievement.Id), Ct);
        await _repository.Received(1).AddXpTransactionAsync(Arg.Is<XpTransactionEntity>(t => t.UserId == userId && t.Amount == 25 && t.Reason.Contains("Century")), Ct);
        await _repository.Received(1).SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task EvaluateAchievementsAsync_ZeroRewardAchievement_UnlocksWithoutAnXpTransaction()
    {
        var userId = Guid.NewGuid();
        var achievement = new AchievementEntity { Id = Guid.NewGuid(), Name = "No bonus", RequiredXp = 0, XpReward = 0 };
        _repository.GetTotalXpAsync(userId, Arg.Any<CancellationToken>()).Returns(0);
        _repository.GetAllAchievementsAsync(Arg.Any<CancellationToken>()).Returns(new List<AchievementEntity> { achievement });
        _repository.HasUserAchievementAsync(userId, achievement.Id, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _service.EvaluateAchievementsAsync(userId, Ct);

        Assert.Single(result);
        await _repository.DidNotReceive().AddXpTransactionAsync(Arg.Any<XpTransactionEntity>(), Ct);
    }
}
