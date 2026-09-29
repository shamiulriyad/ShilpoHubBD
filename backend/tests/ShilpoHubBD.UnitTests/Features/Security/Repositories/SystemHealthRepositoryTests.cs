using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Security.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Security")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class SystemHealthRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public SystemHealthRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task CanConnectAsync_ReachableDatabase_ReturnsTrue()
    {
        await using var db = await _database.BeginAsync();

        Assert.True(await new SystemHealthRepository(db.NewContext()).CanConnectAsync(Ct));
    }

    [Fact]
    public async Task CanConnectAsync_UnreachableServer_ReturnsFalseInsteadOfThrowing()
    {
        var options = new DbContextOptionsBuilder<ShilpoHubDbContext>()
            .UseNpgsql("Host=127.0.0.1;Port=1;Database=nowhere;Username=nobody;Timeout=2")
            .Options;
        await using var context = new ShilpoHubDbContext(options);

        Assert.False(await new SystemHealthRepository(context).CanConnectAsync(Ct));
    }

    [Fact]
    public async Task GetCountsAsync_MatchesTheTableCounts()
    {
        await using var db = await _database.BeginAsync();
        var check = db.NewContext();

        var counts = await new SystemHealthRepository(db.NewContext()).GetCountsAsync(Ct);

        Assert.Equal(await check.Users.CountAsync(Ct), counts.Users);
        Assert.Equal(await check.Orders.CountAsync(Ct), counts.Orders);
        Assert.Equal(await check.Products.CountAsync(Ct), counts.Products);
    }

    [Fact]
    public async Task GetCountsAsync_NewUser_IncreasesTheUserCountByOne()
    {
        await using var db = await _database.BeginAsync();
        var repository = new SystemHealthRepository(db.NewContext());
        var before = await repository.GetCountsAsync(Ct);
        var context = db.NewContext();
        context.Users.Add(TestUsers.Create());
        await context.SaveChangesAsync(Ct);

        var after = await repository.GetCountsAsync(Ct);

        Assert.Equal(before.Users + 1, after.Users);
        Assert.Equal(before.Orders, after.Orders);
        Assert.Equal(before.Products, after.Products);
    }
}
