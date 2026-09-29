using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Admin.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Admin")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class AdminUserRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public AdminUserRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<User> AddAsync(
        ShilpoHubDbContext context, string fullName, string email, bool isActive = true, params string[] roleNames)
    {
        var user = TestUsers.Create(email, fullName, isActive);
        foreach (var name in roleNames)
        {
            var role = await context.Roles.SingleOrDefaultAsync(r => r.Name == name, Ct) ?? TestUsers.Role(name);
            user.WithRoles(role);
        }

        context.Users.Add(user);
        await context.SaveChangesAsync(Ct);
        return user;
    }

    [Fact]
    public async Task GetPagedAsync_NoFilters_ReturnsNewestFirstWithTotalAndRoles()
    {
        await using var db = await _database.BeginAsync();
        var older = await AddAsync(db.NewContext(), "Alice", "alice@example.com", roleNames: RoleNames.Customer);
        var context = db.NewContext();
        context.Entry(older).Property(u => u.CreatedAt).CurrentValue = DateTime.UtcNow.AddDays(-1);
        await context.SaveChangesAsync(Ct);
        var newer = await AddAsync(db.NewContext(), "Bob", "bob@example.com", roleNames: RoleNames.Producer);

        var (items, total) = await new AdminUserRepository(db.NewContext()).GetPagedAsync(new AdminUserQueryParameters { PageSize = 100 }, Ct);

        Assert.True(total >= 2);
        var index = items.FindIndex(u => u.Id == newer.Id);
        var olderIndex = items.FindIndex(u => u.Id == older.Id);
        Assert.True(index < olderIndex);
        Assert.Equal(RoleNames.Producer, Assert.Single(items.Single(u => u.Id == newer.Id).UserRoles).Role.Name);
    }

    [Fact]
    public async Task GetPagedAsync_Search_MatchesNameOrEmailCaseInsensitively()
    {
        await using var db = await _database.BeginAsync();
        var target = await AddAsync(db.NewContext(), "Rahima Begum", "rahima@example.com");
        await AddAsync(db.NewContext(), "Karim Sheikh", "karim@example.com");

        var (items, total) = await new AdminUserRepository(db.NewContext())
            .GetPagedAsync(new AdminUserQueryParameters { Search = "RAHIMA" }, Ct);

        Assert.Equal(1, total);
        Assert.Equal(target.Id, Assert.Single(items).Id);
    }

    [Fact]
    public async Task GetPagedAsync_ByRole_ReturnsOnlyUsersHoldingThatRole()
    {
        await using var db = await _database.BeginAsync();
        var producer = await AddAsync(db.NewContext(), "Producer User", "p@example.com", roleNames: RoleNames.Producer);
        await AddAsync(db.NewContext(), "Customer User", "c@example.com", roleNames: RoleNames.Customer);

        var (items, total) = await new AdminUserRepository(db.NewContext())
            .GetPagedAsync(new AdminUserQueryParameters { Role = RoleNames.Producer }, Ct);

        Assert.Equal(1, total);
        Assert.Equal(producer.Id, Assert.Single(items).Id);
    }

    [Fact]
    public async Task GetPagedAsync_ByIsActive_ReturnsOnlyMatchingUsers()
    {
        await using var db = await _database.BeginAsync();
        var active = await AddAsync(db.NewContext(), "Active User", "active@example.com", isActive: true);
        await AddAsync(db.NewContext(), "Suspended User", "suspended@example.com", isActive: false);

        var (items, total) = await new AdminUserRepository(db.NewContext())
            .GetPagedAsync(new AdminUserQueryParameters { IsActive = true }, Ct);

        Assert.Contains(items, u => u.Id == active.Id);
        Assert.DoesNotContain(items, u => !u.IsActive);
        Assert.Equal(items.Count, total);
    }

    [Fact]
    public async Task GetPagedAsync_Paging_ReturnsTheRequestedSlice()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        for (var i = 0; i < 3; i++)
        {
            await AddAsync(context, $"Paging User {i}", $"paging{i}-{Guid.NewGuid():N}@example.com");
        }

        var (page1, total) = await new AdminUserRepository(db.NewContext())
            .GetPagedAsync(new AdminUserQueryParameters { Search = "Paging User", Page = 1, PageSize = 2 }, Ct);
        var (page2, _) = await new AdminUserRepository(db.NewContext())
            .GetPagedAsync(new AdminUserQueryParameters { Search = "Paging User", Page = 2, PageSize = 2 }, Ct);

        Assert.Equal(3, total);
        Assert.Equal(2, page1.Count);
        Assert.Single(page2);
        Assert.Empty(page1.Select(u => u.Id).Intersect(page2.Select(u => u.Id)));
    }

    [Fact]
    public async Task GetByIdWithRolesAsync_LoadsRoles()
    {
        await using var db = await _database.BeginAsync();
        var user = await AddAsync(db.NewContext(), "Rahima Begum", "rahima2@example.com", roleNames: RoleNames.Tourist);

        var found = await new AdminUserRepository(db.NewContext()).GetByIdWithRolesAsync(user.Id, Ct);

        Assert.NotNull(found);
        Assert.Equal(RoleNames.Tourist, Assert.Single(found.UserRoles).Role.Name);
    }

    [Fact]
    public async Task GetByIdWithRolesAsync_UnknownId_ReturnsNull()
    {
        await using var db = await _database.BeginAsync();

        Assert.Null(await new AdminUserRepository(db.NewContext()).GetByIdWithRolesAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsChangesMadeThroughTheContext()
    {
        await using var db = await _database.BeginAsync();
        var user = await AddAsync(db.NewContext(), "Karim Sheikh", "karim2@example.com");
        var loadContext = db.NewContext();
        var repository = new AdminUserRepository(loadContext);
        var loaded = await repository.GetByIdWithRolesAsync(user.Id, Ct);
        loaded!.IsActive = false;

        await repository.SaveChangesAsync(Ct);

        Assert.False((await db.NewContext().Users.SingleAsync(u => u.Id == user.Id, Ct)).IsActive);
    }
}
