using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;
using AchievementEntity = ShilpoHubBD.Domain.Entities.Achievement.Achievement;
using UserAchievementEntity = ShilpoHubBD.Domain.Entities.Achievement.UserAchievement;
using XpTransactionEntity = ShilpoHubBD.Domain.Entities.Achievement.XpTransaction;

namespace ShilpoHubBD.UnitTests.Features.Achievement.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Achievement")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class AchievementRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public AchievementRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetTotalXpAsync_NoTransactions_ReturnsZero()
    {
        await using var scope = await _database.BeginAsync();
        var repository = new AchievementRepository(scope.NewContext());

        Assert.Equal(0, await repository.GetTotalXpAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetTotalXpAsync_SumsOnlyThatUsersTransactions()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var user = TestUsers.Create();
        var other = TestUsers.Create();
        seedContext.Users.AddRange(user, other);
        seedContext.XpTransactions.AddRange(
            new XpTransactionEntity { Id = Guid.NewGuid(), UserId = user.Id, Amount = 30, Reason = "x", CreatedAt = DateTime.UtcNow },
            new XpTransactionEntity { Id = Guid.NewGuid(), UserId = user.Id, Amount = 20, Reason = "x", CreatedAt = DateTime.UtcNow },
            new XpTransactionEntity { Id = Guid.NewGuid(), UserId = other.Id, Amount = 100, Reason = "x", CreatedAt = DateTime.UtcNow });
        await seedContext.SaveChangesAsync(Ct);

        var repository = new AchievementRepository(scope.NewContext());

        Assert.Equal(50, await repository.GetTotalXpAsync(user.Id, Ct));
    }

    [Fact]
    public async Task GetXpHistoryAsync_ReturnsOnlyThatUsersTransactions_NewestFirst()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var user = TestUsers.Create();
        var other = TestUsers.Create();
        seedContext.Users.AddRange(user, other);
        var older = new XpTransactionEntity { Id = Guid.NewGuid(), UserId = user.Id, Amount = 10, Reason = "x", CreatedAt = DateTime.UtcNow.AddDays(-1) };
        var newer = new XpTransactionEntity { Id = Guid.NewGuid(), UserId = user.Id, Amount = 20, Reason = "x", CreatedAt = DateTime.UtcNow };
        var somebodyElses = new XpTransactionEntity { Id = Guid.NewGuid(), UserId = other.Id, Amount = 5, Reason = "x", CreatedAt = DateTime.UtcNow };
        seedContext.XpTransactions.AddRange(older, newer, somebodyElses);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new AchievementRepository(scope.NewContext());
        var result = await repository.GetXpHistoryAsync(user.Id, Ct);

        Assert.Equal(new[] { newer.Id, older.Id }, result.Select(t => t.Id));
    }

    [Fact]
    public async Task AddXpTransactionAsync_ThenSaveChangesAsync_PersistsTheTransaction()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var user = TestUsers.Create();
        seedContext.Users.Add(user);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new AchievementRepository(scope.NewContext());
        await repository.AddXpTransactionAsync(new XpTransactionEntity { Id = Guid.NewGuid(), UserId = user.Id, Amount = 15, Reason = "Bonus", CreatedAt = DateTime.UtcNow }, Ct);
        await repository.SaveChangesAsync(Ct);

        Assert.Equal(15, await new AchievementRepository(scope.NewContext()).GetTotalXpAsync(user.Id, Ct));
    }

    [Fact]
    public async Task GetAllAchievementsAsync_OrdersByRequiredXp()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var high = new AchievementEntity { Id = Guid.NewGuid(), Name = "High " + Guid.NewGuid().ToString("N")[..8], Description = "x", RequiredXp = 500, CreatedAt = DateTime.UtcNow };
        var low = new AchievementEntity { Id = Guid.NewGuid(), Name = "Low " + Guid.NewGuid().ToString("N")[..8], Description = "x", RequiredXp = 10, CreatedAt = DateTime.UtcNow };
        seedContext.Achievements.AddRange(high, low);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new AchievementRepository(scope.NewContext());
        var result = await repository.GetAllAchievementsAsync(Ct);

        var lowIndex = result.FindIndex(a => a.Id == low.Id);
        var highIndex = result.FindIndex(a => a.Id == high.Id);
        Assert.True(lowIndex < highIndex);
    }

    [Fact]
    public async Task GetAchievementByIdAsync_UnknownId_ReturnsNull()
    {
        await using var scope = await _database.BeginAsync();
        var repository = new AchievementRepository(scope.NewContext());

        Assert.Null(await repository.GetAchievementByIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task AddAchievementAsync_ThenSaveChangesAsync_PersistsTheAchievement()
    {
        await using var scope = await _database.BeginAsync();
        var repository = new AchievementRepository(scope.NewContext());
        var achievement = new AchievementEntity { Id = Guid.NewGuid(), Name = "First Sale", Description = "x", RequiredXp = 100, CreatedAt = DateTime.UtcNow };

        await repository.AddAchievementAsync(achievement, Ct);
        await repository.SaveChangesAsync(Ct);

        var reloaded = await new AchievementRepository(scope.NewContext()).GetAchievementByIdAsync(achievement.Id, Ct);
        Assert.NotNull(reloaded);
        Assert.Equal("First Sale", reloaded!.Name);
    }

    [Fact]
    public async Task GetUserAchievementsAsync_ReturnsOnlyThatUsersUnlocksWithAchievementLoaded_NewestFirst()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var user = TestUsers.Create();
        var other = TestUsers.Create();
        seedContext.Users.AddRange(user, other);
        var achievement = new AchievementEntity { Id = Guid.NewGuid(), Name = "First Sale", Description = "x", CreatedAt = DateTime.UtcNow };
        seedContext.Achievements.Add(achievement);
        var older = new UserAchievementEntity { Id = Guid.NewGuid(), UserId = user.Id, AchievementId = achievement.Id, UnlockedAt = DateTime.UtcNow.AddDays(-1) };
        var otherAchievement = new AchievementEntity { Id = Guid.NewGuid(), Name = "Other", Description = "x", CreatedAt = DateTime.UtcNow };
        seedContext.Achievements.Add(otherAchievement);
        var newer = new UserAchievementEntity { Id = Guid.NewGuid(), UserId = user.Id, AchievementId = otherAchievement.Id, UnlockedAt = DateTime.UtcNow };
        var somebodyElses = new UserAchievementEntity { Id = Guid.NewGuid(), UserId = other.Id, AchievementId = achievement.Id, UnlockedAt = DateTime.UtcNow };
        seedContext.UserAchievements.AddRange(older, newer, somebodyElses);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new AchievementRepository(scope.NewContext());
        var result = await repository.GetUserAchievementsAsync(user.Id, Ct);

        Assert.Equal(new[] { newer.Id, older.Id }, result.Select(ua => ua.Id));
        Assert.Equal("Other", result[0].Achievement.Name);
    }

    [Fact]
    public async Task HasUserAchievementAsync_ReflectsWhetherThatUnlockExists()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var user = TestUsers.Create();
        seedContext.Users.Add(user);
        var achievement = new AchievementEntity { Id = Guid.NewGuid(), Name = "First Sale", Description = "x", CreatedAt = DateTime.UtcNow };
        seedContext.Achievements.Add(achievement);
        seedContext.UserAchievements.Add(new UserAchievementEntity { Id = Guid.NewGuid(), UserId = user.Id, AchievementId = achievement.Id, UnlockedAt = DateTime.UtcNow });
        await seedContext.SaveChangesAsync(Ct);

        var repository = new AchievementRepository(scope.NewContext());

        Assert.True(await repository.HasUserAchievementAsync(user.Id, achievement.Id, Ct));
        Assert.False(await repository.HasUserAchievementAsync(user.Id, Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task AddUserAchievementAsync_SameUserAndAchievementTwice_ViolatesTheUniqueIndex()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var user = TestUsers.Create();
        seedContext.Users.Add(user);
        var achievement = new AchievementEntity { Id = Guid.NewGuid(), Name = "First Sale", Description = "x", CreatedAt = DateTime.UtcNow };
        seedContext.Achievements.Add(achievement);
        seedContext.UserAchievements.Add(new UserAchievementEntity { Id = Guid.NewGuid(), UserId = user.Id, AchievementId = achievement.Id, UnlockedAt = DateTime.UtcNow });
        await seedContext.SaveChangesAsync(Ct);

        var repository = new AchievementRepository(scope.NewContext());
        await repository.AddUserAchievementAsync(new UserAchievementEntity { Id = Guid.NewGuid(), UserId = user.Id, AchievementId = achievement.Id, UnlockedAt = DateTime.UtcNow }, Ct);

        await Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateException>(() => repository.SaveChangesAsync(Ct));
    }
}
