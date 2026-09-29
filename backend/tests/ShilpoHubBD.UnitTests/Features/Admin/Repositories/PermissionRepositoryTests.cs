using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Entities.Admin;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Admin.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Admin")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class PermissionRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public PermissionRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Permission MakePermission(string code, string module = "Users", string name = "Permission") => new()
    {
        Id = Guid.NewGuid(), Code = code, Name = name, Module = module, CreatedAt = DateTime.UtcNow,
    };

    [Fact]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsThePermission()
    {
        await using var db = await _database.BeginAsync();
        var permission = MakePermission($"unit.test.{Guid.NewGuid():N}");
        var repository = new PermissionRepository(db.NewContext());

        await repository.AddAsync(permission, Ct);
        await repository.SaveChangesAsync(Ct);

        Assert.True(await db.NewContext().Permissions.AnyAsync(p => p.Id == permission.Id, Ct));
    }

    [Fact]
    public async Task GetAllAsync_OrdersByModuleThenName()
    {
        await using var db = await _database.BeginAsync();
        var tag = Guid.NewGuid().ToString("N");
        var context = db.NewContext();
        context.Permissions.AddRange(
            MakePermission($"z.{tag}", module: $"Zeta-{tag}", name: "Z"),
            MakePermission($"a2.{tag}", module: $"Alpha-{tag}", name: "B"),
            MakePermission($"a1.{tag}", module: $"Alpha-{tag}", name: "A"));
        await context.SaveChangesAsync(Ct);

        var all = await new PermissionRepository(db.NewContext()).GetAllAsync(Ct);
        var ours = all.Where(p => p.Code.EndsWith(tag, StringComparison.Ordinal)).ToList();

        Assert.Equal(new[] { $"a1.{tag}", $"a2.{tag}", $"z.{tag}" }, ours.Select(p => p.Code));
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var db = await _database.BeginAsync();

        Assert.Null(await new PermissionRepository(db.NewContext()).GetByIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetByCodeAsync_ExactMatchOnly()
    {
        await using var db = await _database.BeginAsync();
        var code = $"exact.{Guid.NewGuid():N}";
        var context = db.NewContext();
        context.Permissions.Add(MakePermission(code));
        await context.SaveChangesAsync(Ct);
        var repository = new PermissionRepository(db.NewContext());

        Assert.NotNull(await repository.GetByCodeAsync(code, Ct));
        Assert.Null(await repository.GetByCodeAsync(code.ToUpperInvariant(), Ct));
    }

    [Fact]
    public async Task GetByCodesAsync_ReturnsOnlyTheMatchingCodes()
    {
        await using var db = await _database.BeginAsync();
        var tag = Guid.NewGuid().ToString("N");
        var context = db.NewContext();
        context.Permissions.AddRange(MakePermission($"one.{tag}"), MakePermission($"two.{tag}"), MakePermission($"three.{tag}"));
        await context.SaveChangesAsync(Ct);

        var found = await new PermissionRepository(db.NewContext())
            .GetByCodesAsync(new[] { $"one.{tag}", $"three.{tag}", $"ghost.{tag}" }, Ct);

        Assert.Equal(new[] { $"one.{tag}", $"three.{tag}" }, found.Select(p => p.Code).Order());
    }

    [Fact]
    public async Task Remove_ThenSaveChangesAsync_DeletesThePermission()
    {
        await using var db = await _database.BeginAsync();
        var permission = MakePermission($"delete.{Guid.NewGuid():N}");
        var context = db.NewContext();
        context.Permissions.Add(permission);
        await context.SaveChangesAsync(Ct);
        var repository = new PermissionRepository(db.NewContext());
        var loaded = await repository.GetByIdAsync(permission.Id, Ct);

        repository.Remove(loaded!);
        await repository.SaveChangesAsync(Ct);

        Assert.False(await db.NewContext().Permissions.AnyAsync(p => p.Id == permission.Id, Ct));
    }

    [Fact]
    public async Task GetRoleByIdAsync_UnknownId_ReturnsNull()
    {
        await using var db = await _database.BeginAsync();

        Assert.Null(await new PermissionRepository(db.NewContext()).GetRoleByIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task RolePermissionLifecycle_AddGetCountAndRemove_WorkTogether()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var role = TestUsers.Role(TestUsers.UniqueRoleName());
        var permission = MakePermission($"lifecycle.{Guid.NewGuid():N}");
        context.Roles.Add(role);
        context.Permissions.Add(permission);
        await context.SaveChangesAsync(Ct);
        var grantedBy = TestUsers.Create();
        context.Users.Add(grantedBy);
        await context.SaveChangesAsync(Ct);
        var repository = new PermissionRepository(db.NewContext());

        await repository.AddRolePermissionsAsync(
            new[] { new RolePermission { Id = Guid.NewGuid(), RoleId = role.Id, PermissionId = permission.Id, GrantedAt = DateTime.UtcNow, GrantedByUserId = grantedBy.Id } },
            Ct);
        await repository.SaveChangesAsync(Ct);

        Assert.Equal(1, await new PermissionRepository(db.NewContext()).CountPermissionsForRoleAsync(role.Id, Ct));
        var granted = await new PermissionRepository(db.NewContext()).GetRolePermissionsAsync(role.Id, Ct);
        Assert.Equal(permission.Code, Assert.Single(granted).Permission.Code);

        var removeRepository = new PermissionRepository(db.NewContext());
        removeRepository.RemoveRolePermissions(await removeRepository.GetRolePermissionsAsync(role.Id, Ct));
        await removeRepository.SaveChangesAsync(Ct);

        Assert.Equal(0, await new PermissionRepository(db.NewContext()).CountPermissionsForRoleAsync(role.Id, Ct));
    }

    [Fact]
    public async Task CountUsersInRoleAsync_CountsHowManyUsersHoldTheRole()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var role = TestUsers.Role(TestUsers.UniqueRoleName());
        context.Roles.Add(role);
        context.Users.Add(TestUsers.Create().WithRoles(role));
        context.Users.Add(TestUsers.Create().WithRoles(role));
        await context.SaveChangesAsync(Ct);

        Assert.Equal(2, await new PermissionRepository(db.NewContext()).CountUsersInRoleAsync(role.Id, Ct));
    }
}
