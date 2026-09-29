using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Entities.Security;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Security.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Security")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class AuditLogRepositoryTests
{
    private static readonly DateTime Now = DateTime.UtcNow;
    private static readonly Guid AliceId = Guid.NewGuid();
    private static readonly Guid BobId = Guid.NewGuid();

    private readonly TestDatabaseFixture _database;

    public AuditLogRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static AuditLog Log(string action, string entityType, Guid? actor, string actorName, string description, DateTime createdAt) => new()
    {
        Id = Guid.NewGuid(),
        Action = action,
        EntityType = entityType,
        ActorUserId = actor,
        ActorName = actorName,
        Description = description,
        CreatedAt = createdAt,
    };

    /// <summary>Four logs spread over four days, newest first: approve, deactivate, reject, system backup.</summary>
    private static async Task SeedAsync(ShilpoHubDbContext context)
    {
        context.AuditLogs.AddRange(
            Log("Product.Approved", "Product", AliceId, "Alice Admin", "Approved Jamdani Saree.", Now),
            Log("AdminUser.Deactivated", "User", BobId, "Bob Admin", "Deactivated a spam account.", Now.AddDays(-1)),
            Log("Product.Rejected", "Product", AliceId, "Alice Admin", "Rejected Nakshi Kantha listing.", Now.AddDays(-2)),
            Log("Backup.Completed", "Backup", null, "System", "Nightly backup finished.", Now.AddDays(-3)));
        await context.SaveChangesAsync(Ct);
    }

    private async Task<(List<AuditLog> Items, int Total)> QueryAsync(AuditLogQueryParameters query)
    {
        await using var db = await _database.BeginAsync();
        await SeedAsync(db.NewContext());
        return await new AuditLogRepository(db.NewContext()).GetPagedAsync(query, Ct);
    }

    [Fact]
    public async Task GetPagedAsync_NoFilters_ReturnsEverythingNewestFirst()
    {
        var (items, total) = await QueryAsync(new AuditLogQueryParameters { Page = 1, PageSize = 20 });

        Assert.Equal(4, total);
        Assert.Equal(new[] { "Product.Approved", "AdminUser.Deactivated", "Product.Rejected", "Backup.Completed" }, items.Select(l => l.Action));
    }

    [Fact]
    public async Task GetPagedAsync_ByAction_MatchesExactly()
    {
        var (items, total) = await QueryAsync(new AuditLogQueryParameters { Action = "Product.Approved" });

        Assert.Equal(1, total);
        Assert.Equal("Approved Jamdani Saree.", Assert.Single(items).Description);
    }

    [Fact]
    public async Task GetPagedAsync_ByEntityType_ReturnsOnlyThatType()
    {
        var (items, total) = await QueryAsync(new AuditLogQueryParameters { EntityType = "Product" });

        Assert.Equal(2, total);
        Assert.All(items, l => Assert.Equal("Product", l.EntityType));
    }

    [Fact]
    public async Task GetPagedAsync_ByActor_ReturnsOnlyThatActorsActions()
    {
        var (items, total) = await QueryAsync(new AuditLogQueryParameters { ActorUserId = AliceId });

        Assert.Equal(2, total);
        Assert.All(items, l => Assert.Equal(AliceId, l.ActorUserId));
    }

    [Theory]
    [InlineData("jamdani", "Product.Approved")]
    [InlineData("  NIGHTLY  ", "Backup.Completed")]
    [InlineData("bob", "AdminUser.Deactivated")]
    public async Task GetPagedAsync_Search_MatchesDescriptionOrActorNameIgnoringCase(string search, string expectedAction)
    {
        var (items, _) = await QueryAsync(new AuditLogQueryParameters { Search = search });

        Assert.Equal(expectedAction, Assert.Single(items).Action);
    }

    [Fact]
    public async Task GetPagedAsync_DateRange_IsInclusiveAtBothEnds()
    {
        var (items, total) = await QueryAsync(new AuditLogQueryParameters { From = Now.AddDays(-2).AddMinutes(-1), To = Now.AddDays(-1).AddMinutes(1) });

        Assert.Equal(2, total);
        Assert.Equal(new[] { "AdminUser.Deactivated", "Product.Rejected" }, items.Select(l => l.Action));
    }

    [Fact]
    public async Task GetPagedAsync_CombinedFilters_MustAllMatch()
    {
        var (items, total) = await QueryAsync(new AuditLogQueryParameters { EntityType = "Product", ActorUserId = AliceId, Search = "rejected" });

        Assert.Equal(1, total);
        Assert.Equal("Product.Rejected", Assert.Single(items).Action);
    }

    [Fact]
    public async Task GetPagedAsync_Paging_ReturnsTheRequestedSliceWithTheFullTotal()
    {
        var (items, total) = await QueryAsync(new AuditLogQueryParameters { Page = 2, PageSize = 3 });

        Assert.Equal(4, total);
        Assert.Equal("Backup.Completed", Assert.Single(items).Action);
    }

    [Fact]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsTheLog()
    {
        await using var db = await _database.BeginAsync();
        var log = Log("Role.Assigned", "User", AliceId, "Alice Admin", "Gave Producer role.", Now);
        var repository = new AuditLogRepository(db.NewContext());

        await repository.AddAsync(log, Ct);
        await repository.SaveChangesAsync(Ct);

        Assert.True(await db.NewContext().AuditLogs.AnyAsync(l => l.Id == log.Id, Ct));
    }
}
