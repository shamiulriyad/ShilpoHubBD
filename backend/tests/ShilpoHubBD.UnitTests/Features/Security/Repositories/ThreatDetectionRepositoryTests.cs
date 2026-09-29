using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Security;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Security.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Security")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class ThreatDetectionRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public ThreatDetectionRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static LoginAttempt Attempt(string? ip, bool succeeded, DateTime createdAt, string email = "someone@example.com") => new()
    {
        Id = Guid.NewGuid(), Email = email, IpAddress = ip, Succeeded = succeeded, CreatedAt = createdAt,
    };

    private static BlockedIpAddress Block(User admin, string ip, DateTime? expiresAt, DateTime? createdAt = null) => new()
    {
        Id = Guid.NewGuid(), IpAddress = ip, Reason = "test", BlockedByUserId = admin.Id,
        ExpiresAt = expiresAt, CreatedAt = createdAt ?? DateTime.UtcNow,
    };

    private static async Task<User> SeedAdminAsync(ShilpoHubDbContext context)
    {
        var admin = TestUsers.Create(fullName: "Admin Person");
        context.Users.Add(admin);
        await context.SaveChangesAsync(Ct);
        return admin;
    }

    private static IEnumerable<LoginAttempt> Failures(string ip, int count, DateTime at)
        => Enumerable.Range(0, count).Select(i => Attempt(ip, false, at.AddSeconds(-i)));

    // ---------- login attempts ----------

    [Fact]
    public async Task AddLoginAttemptAsync_ThenSaveChangesAsync_PersistsTheAttempt()
    {
        await using var db = await _database.BeginAsync();
        var attempt = Attempt("203.0.113.1", false, DateTime.UtcNow);
        var repository = new ThreatDetectionRepository(db.NewContext());

        await repository.AddLoginAttemptAsync(attempt, Ct);
        await repository.SaveChangesAsync(Ct);

        Assert.True(await db.NewContext().LoginAttempts.AnyAsync(a => a.Id == attempt.Id, Ct));
    }

    [Fact]
    public async Task GetFailedLoginsPagedAsync_ReturnsOnlyFailuresNewestFirstWithTotal()
    {
        await using var db = await _database.BeginAsync();
        var now = DateTime.UtcNow;
        var context = db.NewContext();
        context.LoginAttempts.AddRange(
            Attempt("203.0.113.1", false, now.AddMinutes(-2), "old@example.com"),
            Attempt("203.0.113.1", true, now.AddMinutes(-1), "ok@example.com"),
            Attempt("203.0.113.2", false, now, "new@example.com"));
        await context.SaveChangesAsync(Ct);

        var (items, total) = await new ThreatDetectionRepository(db.NewContext()).GetFailedLoginsPagedAsync(1, 10, Ct);

        Assert.Equal(2, total);
        Assert.Equal(new[] { "new@example.com", "old@example.com" }, items.Select(a => a.Email));
    }

    [Fact]
    public async Task GetFailedLoginsPagedAsync_SecondPage_SkipsTheFirstPage()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        context.LoginAttempts.AddRange(Failures("203.0.113.1", 3, DateTime.UtcNow));
        await context.SaveChangesAsync(Ct);

        var (items, total) = await new ThreatDetectionRepository(db.NewContext()).GetFailedLoginsPagedAsync(2, 2, Ct);

        Assert.Equal(3, total);
        Assert.Single(items);
    }

    // ---------- suspicious IPs ----------

    [Fact]
    public async Task GetSuspiciousIpsAsync_CountsRecentFailuresPerIpAtOrAboveTheThresholdBusiestFirst()
    {
        await using var db = await _database.BeginAsync();
        var now = DateTime.UtcNow;
        var context = db.NewContext();
        context.LoginAttempts.AddRange(Failures("198.51.100.1", 5, now));
        context.LoginAttempts.AddRange(Failures("198.51.100.2", 7, now));
        context.LoginAttempts.AddRange(Failures("198.51.100.3", 4, now));
        await context.SaveChangesAsync(Ct);

        var result = await new ThreatDetectionRepository(db.NewContext()).GetSuspiciousIpsAsync(now.AddHours(-1), 5, Ct);

        Assert.Equal(new[] { ("198.51.100.2", 7), ("198.51.100.1", 5) }, result);
    }

    [Fact]
    public async Task GetSuspiciousIpsAsync_IgnoresSuccessesOldAttemptsAndAttemptsWithoutAnIp()
    {
        await using var db = await _database.BeginAsync();
        var now = DateTime.UtcNow;
        var context = db.NewContext();
        context.LoginAttempts.AddRange(Failures("198.51.100.9", 4, now));
        context.LoginAttempts.Add(Attempt("198.51.100.9", true, now));
        context.LoginAttempts.Add(Attempt("198.51.100.9", false, now.AddHours(-2)));
        context.LoginAttempts.AddRange(Enumerable.Range(0, 6).Select(_ => Attempt(null, false, now)));
        await context.SaveChangesAsync(Ct);

        var result = await new ThreatDetectionRepository(db.NewContext()).GetSuspiciousIpsAsync(now.AddHours(-1), 5, Ct);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSuspiciousIpsAsync_LeavesOutCurrentlyBlockedIpsButNotExpiredBlocks()
    {
        await using var db = await _database.BeginAsync();
        var now = DateTime.UtcNow;
        var context = db.NewContext();
        var admin = await SeedAdminAsync(context);
        context.LoginAttempts.AddRange(Failures("198.51.100.10", 6, now));
        context.LoginAttempts.AddRange(Failures("198.51.100.11", 6, now));
        context.LoginAttempts.AddRange(Failures("198.51.100.12", 6, now));
        context.BlockedIpAddresses.AddRange(
            Block(admin, "198.51.100.10", expiresAt: null),
            Block(admin, "198.51.100.11", expiresAt: now.AddMinutes(-5)));
        await context.SaveChangesAsync(Ct);

        var result = await new ThreatDetectionRepository(db.NewContext()).GetSuspiciousIpsAsync(now.AddHours(-1), 5, Ct);

        Assert.Equal(new[] { "198.51.100.11", "198.51.100.12" }, result.Select(r => r.IpAddress).Order());
    }

    // ---------- blocked IPs ----------

    [Fact]
    public async Task IsIpBlockedAsync_TrueForPermanentAndUnexpiredBlocksOnly()
    {
        await using var db = await _database.BeginAsync();
        var now = DateTime.UtcNow;
        var context = db.NewContext();
        var admin = await SeedAdminAsync(context);
        context.BlockedIpAddresses.AddRange(
            Block(admin, "192.0.2.1", expiresAt: null),
            Block(admin, "192.0.2.2", expiresAt: now.AddHours(1)),
            Block(admin, "192.0.2.3", expiresAt: now.AddMinutes(-1)));
        await context.SaveChangesAsync(Ct);
        var repository = new ThreatDetectionRepository(db.NewContext());

        Assert.True(await repository.IsIpBlockedAsync("192.0.2.1", Ct));
        Assert.True(await repository.IsIpBlockedAsync("192.0.2.2", Ct));
        Assert.False(await repository.IsIpBlockedAsync("192.0.2.3", Ct));
        Assert.False(await repository.IsIpBlockedAsync("192.0.2.4", Ct));
    }

    [Fact]
    public async Task GetBlockedIpsAsync_ListsEveryBlockNewestFirstWithWhoBlockedIt()
    {
        await using var db = await _database.BeginAsync();
        var now = DateTime.UtcNow;
        var context = db.NewContext();
        var admin = await SeedAdminAsync(context);
        context.BlockedIpAddresses.AddRange(
            Block(admin, "192.0.2.10", expiresAt: null, createdAt: now.AddDays(-1)),
            Block(admin, "192.0.2.11", expiresAt: now.AddMinutes(-1), createdAt: now));
        await context.SaveChangesAsync(Ct);

        var blocks = await new ThreatDetectionRepository(db.NewContext()).GetBlockedIpsAsync(Ct);

        Assert.Equal(new[] { "192.0.2.11", "192.0.2.10" }, blocks.Select(b => b.IpAddress));
        Assert.All(blocks, b => Assert.Equal("Admin Person", b.BlockedBy.FullName));
    }

    [Fact]
    public async Task GetBlockedIpByAddressAsync_MatchesExactlyAndReturnsNullOtherwise()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var admin = await SeedAdminAsync(context);
        context.BlockedIpAddresses.Add(Block(admin, "192.0.2.20", expiresAt: null));
        await context.SaveChangesAsync(Ct);
        var repository = new ThreatDetectionRepository(db.NewContext());

        Assert.NotNull(await repository.GetBlockedIpByAddressAsync("192.0.2.20", Ct));
        Assert.Null(await repository.GetBlockedIpByAddressAsync("192.0.2.2", Ct));
    }

    [Fact]
    public async Task GetBlockedIpByAddressAsync_StillReturnsTheRowAfterItsBlockHasExpired()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var admin = await SeedAdminAsync(context);
        context.BlockedIpAddresses.Add(Block(admin, "192.0.2.21", expiresAt: DateTime.UtcNow.AddDays(-1)));
        await context.SaveChangesAsync(Ct);

        var found = await new ThreatDetectionRepository(db.NewContext()).GetBlockedIpByAddressAsync("192.0.2.21", Ct);

        Assert.NotNull(found);
    }

    [Fact]
    public async Task AddBlockedIpAsync_AndRemoveBlockedIp_PersistThroughSaveChanges()
    {
        await using var db = await _database.BeginAsync();
        var admin = await SeedAdminAsync(db.NewContext());
        var block = Block(admin, "192.0.2.30", expiresAt: null);
        var repository = new ThreatDetectionRepository(db.NewContext());

        await repository.AddBlockedIpAsync(block, Ct);
        await repository.SaveChangesAsync(Ct);
        Assert.True(await db.NewContext().BlockedIpAddresses.AnyAsync(b => b.IpAddress == "192.0.2.30", Ct));

        repository.RemoveBlockedIp(block);
        await repository.SaveChangesAsync(Ct);
        Assert.False(await db.NewContext().BlockedIpAddresses.AnyAsync(b => b.IpAddress == "192.0.2.30", Ct));
    }
}
