using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Auth.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Auth")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class UserRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public UserRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<User> SeedUserAsync(ShilpoHubDbContext context, params string[] roleNames)
    {
        var user = TestUsers.Create();
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
    public async Task GetByIdAsync_ExistingUser_ReturnsItWithoutLoadingRoles()
    {
        await using var db = await _database.BeginAsync();
        var seeded = await SeedUserAsync(db.NewContext(), RoleNames.Customer);

        var found = await new UserRepository(db.NewContext()).GetByIdAsync(seeded.Id, Ct);

        Assert.NotNull(found);
        Assert.Equal(seeded.Email, found.Email);
        Assert.Empty(found.UserRoles);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var db = await _database.BeginAsync();

        Assert.Null(await new UserRepository(db.NewContext()).GetByIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetByIdWithRolesAsync_LoadsEveryRoleWithItsName()
    {
        await using var db = await _database.BeginAsync();
        var seeded = await SeedUserAsync(db.NewContext(), RoleNames.Customer, RoleNames.Producer);

        var found = await new UserRepository(db.NewContext()).GetByIdWithRolesAsync(seeded.Id, Ct);

        Assert.NotNull(found);
        Assert.Equal(new[] { RoleNames.Customer, RoleNames.Producer }, found.UserRoles.Select(ur => ur.Role.Name).Order());
    }

    [Fact]
    public async Task GetByEmailWithRolesAsync_ExactEmail_ReturnsTheUserWithRoles()
    {
        await using var db = await _database.BeginAsync();
        var seeded = await SeedUserAsync(db.NewContext(), RoleNames.Tourist);

        var found = await new UserRepository(db.NewContext()).GetByEmailWithRolesAsync(seeded.Email, Ct);

        Assert.NotNull(found);
        Assert.Equal(seeded.Id, found.Id);
        Assert.Equal(RoleNames.Tourist, Assert.Single(found.UserRoles).Role.Name);
    }

    [Fact]
    public async Task GetByEmailWithRolesAsync_MatchesCaseSensitively_SoCallersMustNormalize()
    {
        await using var db = await _database.BeginAsync();
        var seeded = await SeedUserAsync(db.NewContext());

        var found = await new UserRepository(db.NewContext()).GetByEmailWithRolesAsync(seeded.Email.ToUpperInvariant(), Ct);

        Assert.Null(found);
    }

    [Fact]
    public async Task ExistsByEmailAsync_ReportsWhetherTheEmailIsTaken()
    {
        await using var db = await _database.BeginAsync();
        var seeded = await SeedUserAsync(db.NewContext());
        var repository = new UserRepository(db.NewContext());

        Assert.True(await repository.ExistsByEmailAsync(seeded.Email, Ct));
        Assert.False(await repository.ExistsByEmailAsync("free-" + seeded.Email, Ct));
    }

    [Fact]
    public async Task AnyInRoleAsync_TrueOnlyWhenSomeUserHoldsTheRole()
    {
        await using var db = await _database.BeginAsync();
        var held = TestUsers.UniqueRoleName();
        var unused = TestUsers.UniqueRoleName();
        var context = db.NewContext();
        context.Roles.Add(TestUsers.Role(unused));
        await SeedUserAsync(context, held);
        var repository = new UserRepository(db.NewContext());

        Assert.True(await repository.AnyInRoleAsync(held, Ct));
        Assert.False(await repository.AnyInRoleAsync(unused, Ct));
    }

    [Fact]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsTheUserAndRoles()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var customer = await context.Roles.SingleAsync(r => r.Name == RoleNames.Customer, Ct);
        var user = TestUsers.Create().WithRoles(customer);
        var repository = new UserRepository(context);

        await repository.AddAsync(user, Ct);
        await repository.SaveChangesAsync(Ct);

        var stored = await db.NewContext().Users.Include(u => u.UserRoles).SingleAsync(u => u.Id == user.Id, Ct);
        Assert.Equal(user.Email, stored.Email);
        Assert.Equal(customer.Id, Assert.Single(stored.UserRoles).RoleId);
    }
}
