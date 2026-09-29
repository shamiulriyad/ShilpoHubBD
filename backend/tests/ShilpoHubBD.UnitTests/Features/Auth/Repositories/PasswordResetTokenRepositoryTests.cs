using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Auth.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Auth")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class PasswordResetTokenRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public PasswordResetTokenRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<(User User, PasswordResetToken Token)> SeedAsync(
        ShilpoHubDbContext context, DateTime? expiresAt = null, DateTime? usedAt = null)
    {
        var user = TestUsers.Create();
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "reset-" + Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddMinutes(50),
            UsedAt = usedAt,
        };
        context.Users.Add(user);
        context.PasswordResetTokens.Add(token);
        await context.SaveChangesAsync(Ct);
        return (user, token);
    }

    [Fact]
    public async Task GetActiveByTokenHashAsync_UnusedUnexpiredTokenOfThatUser_IsReturned()
    {
        await using var db = await _database.BeginAsync();
        var (user, token) = await SeedAsync(db.NewContext());

        var found = await new PasswordResetTokenRepository(db.NewContext()).GetActiveByTokenHashAsync(token.TokenHash, user.Id, Ct);

        Assert.NotNull(found);
        Assert.Equal(token.Id, found.Id);
    }

    [Fact]
    public async Task GetActiveByTokenHashAsync_AlreadyUsedToken_ReturnsNull()
    {
        await using var db = await _database.BeginAsync();
        var (user, token) = await SeedAsync(db.NewContext(), usedAt: DateTime.UtcNow.AddMinutes(-1));

        Assert.Null(await new PasswordResetTokenRepository(db.NewContext()).GetActiveByTokenHashAsync(token.TokenHash, user.Id, Ct));
    }

    [Fact]
    public async Task GetActiveByTokenHashAsync_ExpiredToken_ReturnsNull()
    {
        await using var db = await _database.BeginAsync();
        var (user, token) = await SeedAsync(db.NewContext(), expiresAt: DateTime.UtcNow.AddSeconds(-1));

        Assert.Null(await new PasswordResetTokenRepository(db.NewContext()).GetActiveByTokenHashAsync(token.TokenHash, user.Id, Ct));
    }

    [Fact]
    public async Task GetActiveByTokenHashAsync_TokenOfAnotherUser_ReturnsNull()
    {
        await using var db = await _database.BeginAsync();
        var (_, token) = await SeedAsync(db.NewContext());
        var (otherUser, _) = await SeedAsync(db.NewContext());

        Assert.Null(await new PasswordResetTokenRepository(db.NewContext()).GetActiveByTokenHashAsync(token.TokenHash, otherUser.Id, Ct));
    }

    [Fact]
    public async Task GetActiveByTokenHashAsync_UnknownHash_ReturnsNull()
    {
        await using var db = await _database.BeginAsync();
        var (user, _) = await SeedAsync(db.NewContext());

        Assert.Null(await new PasswordResetTokenRepository(db.NewContext()).GetActiveByTokenHashAsync("no-such-hash", user.Id, Ct));
    }

    [Fact]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsTheToken()
    {
        await using var db = await _database.BeginAsync();
        var (user, _) = await SeedAsync(db.NewContext());
        var repository = new PasswordResetTokenRepository(db.NewContext());
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "reset-" + Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };

        await repository.AddAsync(token, Ct);
        await repository.SaveChangesAsync(Ct);

        Assert.True(await db.NewContext().PasswordResetTokens.AnyAsync(t => t.Id == token.Id, Ct));
    }
}
