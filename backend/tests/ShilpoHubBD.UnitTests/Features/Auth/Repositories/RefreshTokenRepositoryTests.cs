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
public class RefreshTokenRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public RefreshTokenRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static RefreshToken Token(Guid userId, DateTime? expiresAt = null, DateTime? revokedAt = null, string? reason = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        TokenHash = "hash-" + Guid.NewGuid().ToString("N"),
        CreatedAt = DateTime.UtcNow.AddHours(-1),
        ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7),
        RevokedAt = revokedAt,
        ReasonRevoked = reason,
    };

    private static async Task<User> SeedUserAsync(ShilpoHubDbContext context, params RefreshToken[] tokens)
    {
        var user = TestUsers.Create();
        context.Users.Add(user);
        await context.SaveChangesAsync(Ct);
        foreach (var token in tokens)
        {
            token.UserId = user.Id;
            context.RefreshTokens.Add(token);
        }

        await context.SaveChangesAsync(Ct);
        return user;
    }

    [Fact]
    public async Task GetByTokenHashAsync_KnownHash_ReturnsTheToken()
    {
        await using var db = await _database.BeginAsync();
        var token = Token(Guid.Empty);
        await SeedUserAsync(db.NewContext(), token);

        var found = await new RefreshTokenRepository(db.NewContext()).GetByTokenHashAsync(token.TokenHash, Ct);

        Assert.NotNull(found);
        Assert.Equal(token.Id, found.Id);
    }

    [Fact]
    public async Task GetByTokenHashAsync_UnknownHash_ReturnsNull()
    {
        await using var db = await _database.BeginAsync();

        Assert.Null(await new RefreshTokenRepository(db.NewContext()).GetByTokenHashAsync("no-such-hash", Ct));
    }

    [Fact]
    public async Task GetByTokenHashAsync_ReturnsRevokedTokensToo_SoReuseCanBeDetected()
    {
        await using var db = await _database.BeginAsync();
        var token = Token(Guid.Empty, revokedAt: DateTime.UtcNow.AddMinutes(-5));
        await SeedUserAsync(db.NewContext(), token);

        var found = await new RefreshTokenRepository(db.NewContext()).GetByTokenHashAsync(token.TokenHash, Ct);

        Assert.NotNull(found);
        Assert.True(found.IsRevoked);
    }

    [Fact]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsTheToken()
    {
        await using var db = await _database.BeginAsync();
        var user = await SeedUserAsync(db.NewContext());
        var token = Token(user.Id);
        var repository = new RefreshTokenRepository(db.NewContext());

        await repository.AddAsync(token, Ct);
        await repository.SaveChangesAsync(Ct);

        Assert.True(await db.NewContext().RefreshTokens.AnyAsync(t => t.TokenHash == token.TokenHash, Ct));
    }

    [Fact]
    public async Task RevokeAllActiveForUserAsync_RevokesOnlyThatUsersActiveTokensAndSaves()
    {
        await using var db = await _database.BeginAsync();
        var earlier = DateTime.UtcNow.AddHours(-3);
        var activeA = Token(Guid.Empty);
        var activeB = Token(Guid.Empty);
        var expired = Token(Guid.Empty, expiresAt: DateTime.UtcNow.AddMinutes(-1));
        var alreadyRevoked = Token(Guid.Empty, revokedAt: earlier, reason: "Rotated on refresh.");
        var otherUsersToken = Token(Guid.Empty);
        var user = await SeedUserAsync(db.NewContext(), activeA, activeB, expired, alreadyRevoked);
        var otherUser = await SeedUserAsync(db.NewContext(), otherUsersToken);

        await new RefreshTokenRepository(db.NewContext())
            .RevokeAllActiveForUserAsync(user.Id, "192.0.2.1", "Password was reset.", Ct);

        var stored = await db.NewContext().RefreshTokens.AsNoTracking()
            .Where(t => t.UserId == user.Id || t.UserId == otherUser.Id)
            .ToDictionaryAsync(t => t.Id, Ct);
        foreach (var id in new[] { activeA.Id, activeB.Id })
        {
            Assert.NotNull(stored[id].RevokedAt);
            Assert.Equal("192.0.2.1", stored[id].RevokedByIp);
            Assert.Equal("Password was reset.", stored[id].ReasonRevoked);
        }

        Assert.Null(stored[expired.Id].RevokedAt);
        Assert.Equal("Rotated on refresh.", stored[alreadyRevoked.Id].ReasonRevoked);
        Assert.Equal(earlier, stored[alreadyRevoked.Id].RevokedAt!.Value, TimeSpan.FromMilliseconds(1));
        Assert.Null(stored[otherUsersToken.Id].RevokedAt);
    }

    [Fact]
    public async Task RevokeAllActiveForUserAsync_UserWithoutTokens_DoesNothing()
    {
        await using var db = await _database.BeginAsync();
        var user = await SeedUserAsync(db.NewContext());

        await new RefreshTokenRepository(db.NewContext()).RevokeAllActiveForUserAsync(user.Id, null, "Logout everywhere.", Ct);

        Assert.False(await db.NewContext().RefreshTokens.AnyAsync(t => t.UserId == user.Id, Ct));
    }
}
