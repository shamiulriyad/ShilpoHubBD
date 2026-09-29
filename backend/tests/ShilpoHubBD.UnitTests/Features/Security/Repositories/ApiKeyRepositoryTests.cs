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
public class ApiKeyRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public ApiKeyRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static ApiKey Key(User creator, DateTime createdAt, string name = "Integration") => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        KeyPrefix = "shb_" + Guid.NewGuid().ToString("N")[..8],
        KeyHash = Guid.NewGuid().ToString("N").ToUpperInvariant(),
        CreatedByUserId = creator.Id,
        IsActive = true,
        CreatedAt = createdAt,
    };

    private static async Task<User> SeedCreatorAsync(ShilpoHubDbContext context, params Func<User, ApiKey>[] keys)
    {
        var creator = TestUsers.Create(fullName: "Admin Person");
        context.Users.Add(creator);
        context.ApiKeys.AddRange(keys.Select(k => k(creator)));
        await context.SaveChangesAsync(Ct);
        return creator;
    }

    [Fact]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsTheKey()
    {
        await using var db = await _database.BeginAsync();
        var creator = await SeedCreatorAsync(db.NewContext());
        var key = Key(creator, DateTime.UtcNow);
        var repository = new ApiKeyRepository(db.NewContext());

        await repository.AddAsync(key, Ct);
        await repository.SaveChangesAsync(Ct);

        var stored = await db.NewContext().ApiKeys.SingleAsync(k => k.Id == key.Id, Ct);
        Assert.Equal(key.KeyHash, stored.KeyHash);
        Assert.Equal(creator.Id, stored.CreatedByUserId);
    }

    [Fact]
    public async Task GetByIdAsync_LoadsTheCreator()
    {
        await using var db = await _database.BeginAsync();
        ApiKey? key = null;
        await SeedCreatorAsync(db.NewContext(), u => key = Key(u, DateTime.UtcNow));

        var found = await new ApiKeyRepository(db.NewContext()).GetByIdAsync(key!.Id, Ct);

        Assert.NotNull(found);
        Assert.Equal("Admin Person", found.CreatedBy.FullName);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var db = await _database.BeginAsync();

        Assert.Null(await new ApiKeyRepository(db.NewContext()).GetByIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsNewestFirstWithTheTotalAndCreators()
    {
        await using var db = await _database.BeginAsync();
        var now = DateTime.UtcNow;
        await SeedCreatorAsync(db.NewContext(),
            u => Key(u, now.AddDays(-2), "oldest"),
            u => Key(u, now, "newest"),
            u => Key(u, now.AddDays(-1), "middle"));

        var (items, total) = await new ApiKeyRepository(db.NewContext()).GetPagedAsync(1, 10, Ct);

        Assert.Equal(3, total);
        Assert.Equal(new[] { "newest", "middle", "oldest" }, items.Select(k => k.Name));
        Assert.All(items, k => Assert.Equal("Admin Person", k.CreatedBy.FullName));
    }

    [Fact]
    public async Task GetPagedAsync_SecondPage_SkipsTheFirstPage()
    {
        await using var db = await _database.BeginAsync();
        var now = DateTime.UtcNow;
        await SeedCreatorAsync(db.NewContext(),
            u => Key(u, now, "k1"), u => Key(u, now.AddMinutes(-1), "k2"), u => Key(u, now.AddMinutes(-2), "k3"));

        var (items, total) = await new ApiKeyRepository(db.NewContext()).GetPagedAsync(2, 2, Ct);

        Assert.Equal(3, total);
        Assert.Equal("k3", Assert.Single(items).Name);
    }
}
