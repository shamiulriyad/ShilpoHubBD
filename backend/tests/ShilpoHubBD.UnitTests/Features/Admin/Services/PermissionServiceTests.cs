using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Services.Admin;
using ShilpoHubBD.Domain.Entities.Admin;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Admin.Services;

[Trait("Feature", "Admin")]
[Trait("Layer", "Service")]
public class PermissionServiceTests
{
    private readonly IPermissionRepository _repository = Substitute.For<IPermissionRepository>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private PermissionService CreateService() => new(_repository);

    private static Permission MakePermission(string code = "users.manage") => new()
    {
        Id = Guid.NewGuid(), Code = code, Name = "Manage users", Module = "Users", CreatedAt = DateTime.UtcNow,
    };

    // ---------- GetAllAsync ----------

    [Fact]
    public async Task GetAllAsync_MapsEveryPermission()
    {
        var permission = MakePermission();
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Permission> { permission });

        var result = await CreateService().GetAllAsync(Ct);

        Assert.Equal(permission.Code, Assert.Single(result).Code);
    }

    // ---------- CreateAsync ----------

    [Fact]
    public async Task CreateAsync_NewCode_StoresATrimmedLowercasedPermission()
    {
        Permission? saved = null;
        await _repository.AddAsync(Arg.Do<Permission>(p => saved = p), Arg.Any<CancellationToken>());
        var before = DateTime.UtcNow;

        var dto = await CreateService().CreateAsync(
            new CreatePermissionRequest { Code = "  Users.Manage  ", Name = "  Manage users  ", Module = "  Users  ", Description = "  desc  " }, Ct);

        Assert.NotNull(saved);
        Assert.Equal("users.manage", saved.Code);
        Assert.Equal("Manage users", saved.Name);
        Assert.Equal("Users", saved.Module);
        Assert.Equal("desc", saved.Description);
        Assert.InRange(saved.CreatedAt, before, DateTime.UtcNow);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal("users.manage", dto.Code);
    }

    [Fact]
    public async Task CreateAsync_NoDescription_StoresNull()
    {
        Permission? saved = null;
        await _repository.AddAsync(Arg.Do<Permission>(p => saved = p), Arg.Any<CancellationToken>());

        await CreateService().CreateAsync(new CreatePermissionRequest { Code = "x", Name = "x", Module = "x", Description = "   " }, Ct);

        Assert.Null(saved!.Description);
    }

    [Fact]
    public async Task CreateAsync_CodeAlreadyExists_ThrowsConflictAndSavesNothing()
    {
        _repository.GetByCodeAsync("users.manage", Arg.Any<CancellationToken>()).Returns(MakePermission());

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().CreateAsync(new CreatePermissionRequest { Code = "Users.Manage", Name = "x", Module = "x" }, Ct));

        Assert.Equal("A permission with this code already exists.", error.Message);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Permission>(), Arg.Any<CancellationToken>());
    }

    // ---------- DeleteAsync ----------

    [Fact]
    public async Task DeleteAsync_ExistingPermission_RemovesIt()
    {
        var permission = MakePermission();
        _repository.GetByIdAsync(permission.Id, Arg.Any<CancellationToken>()).Returns(permission);

        await CreateService().DeleteAsync(permission.Id, Ct);

        _repository.Received(1).Remove(permission);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_UnknownPermission_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().DeleteAsync(Guid.NewGuid(), Ct));

        Assert.Equal("Permission not found.", error.Message);
        _repository.DidNotReceive().Remove(Arg.Any<Permission>());
    }

    // ---------- GetRolesAsync ----------

    [Fact]
    public async Task GetRolesAsync_ReturnsEachRoleWithItsUserAndPermissionCounts()
    {
        var role = TestUsers.Role("Producer");
        _repository.GetAllRolesWithCountsAsync(Arg.Any<CancellationToken>()).Returns(new List<Role> { role });
        _repository.CountUsersInRoleAsync(role.Id, Arg.Any<CancellationToken>()).Returns(7);
        _repository.CountPermissionsForRoleAsync(role.Id, Arg.Any<CancellationToken>()).Returns(3);

        var result = await CreateService().GetRolesAsync(Ct);

        var dto = Assert.Single(result);
        Assert.Equal(role.Id, dto.Id);
        Assert.Equal("Producer", dto.Name);
        Assert.Equal(7, dto.UserCount);
        Assert.Equal(3, dto.PermissionCount);
    }

    [Fact]
    public async Task GetRolesAsync_NoRoles_ReturnsEmpty()
    {
        _repository.GetAllRolesWithCountsAsync(Arg.Any<CancellationToken>()).Returns(new List<Role>());

        Assert.Empty(await CreateService().GetRolesAsync(Ct));
    }

    // ---------- GetRolePermissionsAsync ----------

    [Fact]
    public async Task GetRolePermissionsAsync_ReturnsTheRolesGrantedCodes()
    {
        var role = TestUsers.Role("Producer");
        var permission = MakePermission();
        _repository.GetRoleByIdAsync(role.Id, Arg.Any<CancellationToken>()).Returns(role);
        _repository.GetRolePermissionsAsync(role.Id, Arg.Any<CancellationToken>())
            .Returns(new List<RolePermission> { new() { RoleId = role.Id, Permission = permission, PermissionId = permission.Id } });

        var dto = await CreateService().GetRolePermissionsAsync(role.Id, Ct);

        Assert.Equal(role.Id, dto.RoleId);
        Assert.Equal("Producer", dto.RoleName);
        Assert.Equal(new[] { "users.manage" }, dto.PermissionCodes);
    }

    [Fact]
    public async Task GetRolePermissionsAsync_UnknownRole_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().GetRolePermissionsAsync(Guid.NewGuid(), Ct));

        Assert.Equal("Role not found.", error.Message);
    }

    // ---------- SyncRolePermissionsAsync ----------

    [Fact]
    public async Task SyncRolePermissionsAsync_ReplacesExistingGrantsWithTheRequestedCodes()
    {
        var role = TestUsers.Role("Producer");
        var admin = Guid.NewGuid();
        _repository.GetRoleByIdAsync(role.Id, Arg.Any<CancellationToken>()).Returns(role);
        var keep = MakePermission("products.manage");
        var grant = MakePermission("orders.manage");
        _repository.GetByCodesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Permission> { keep, grant });
        var existing = new List<RolePermission> { new() { RoleId = role.Id, PermissionId = Guid.NewGuid() } };
        _repository.GetRolePermissionsAsync(role.Id, Arg.Any<CancellationToken>()).Returns(existing);
        IEnumerable<RolePermission>? added = null;
        await _repository.AddRolePermissionsAsync(Arg.Do<IEnumerable<RolePermission>>(rp => added = rp.ToList()), Arg.Any<CancellationToken>());
        var before = DateTime.UtcNow;

        var dto = await CreateService().SyncRolePermissionsAsync(
            role.Id, admin, new SyncRolePermissionsRequest { PermissionCodes = new() { "products.manage", "orders.manage" } }, Ct);

        _repository.Received(1).RemoveRolePermissions(existing);
        Assert.NotNull(added);
        Assert.Equal(2, added.Count());
        Assert.All(added, rp => { Assert.Equal(role.Id, rp.RoleId); Assert.Equal(admin, rp.GrantedByUserId); Assert.InRange(rp.GrantedAt, before, DateTime.UtcNow); });
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal(new[] { "products.manage", "orders.manage" }, dto.PermissionCodes);
    }

    [Fact]
    public async Task SyncRolePermissionsAsync_DuplicateCodes_AreLookedUpOnce()
    {
        var role = TestUsers.Role("Producer");
        _repository.GetRoleByIdAsync(role.Id, Arg.Any<CancellationToken>()).Returns(role);
        _repository.GetByCodesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>()).Returns(new List<Permission> { MakePermission() });
        _repository.GetRolePermissionsAsync(role.Id, Arg.Any<CancellationToken>()).Returns(new List<RolePermission>());

        await CreateService().SyncRolePermissionsAsync(
            role.Id, Guid.NewGuid(), new SyncRolePermissionsRequest { PermissionCodes = new() { "users.manage", "users.manage" } }, Ct);

        await _repository.Received(1).GetByCodesAsync(Arg.Is<IEnumerable<string>>(c => c.Count() == 1), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncRolePermissionsAsync_UnknownCode_ThrowsConflictAndSavesNothing()
    {
        var role = TestUsers.Role("Producer");
        _repository.GetRoleByIdAsync(role.Id, Arg.Any<CancellationToken>()).Returns(role);
        _repository.GetByCodesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>()).Returns(new List<Permission>());

        var error = await Assert.ThrowsAsync<ConflictException>(() => CreateService().SyncRolePermissionsAsync(
            role.Id, Guid.NewGuid(), new SyncRolePermissionsRequest { PermissionCodes = new() { "ghost.code" } }, Ct));

        Assert.Equal("Unknown permission code(s): ghost.code.", error.Message);
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _repository.DidNotReceive().RemoveRolePermissions(Arg.Any<IEnumerable<RolePermission>>());
    }

    [Fact]
    public async Task SyncRolePermissionsAsync_UnknownRole_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().SyncRolePermissionsAsync(
            Guid.NewGuid(), Guid.NewGuid(), new SyncRolePermissionsRequest { PermissionCodes = new() }, Ct));

        Assert.Equal("Role not found.", error.Message);
    }

    [Fact]
    public async Task SyncRolePermissionsAsync_EmptyCodeList_RemovesAllGrantsWithoutAddingAny()
    {
        var role = TestUsers.Role("Producer");
        _repository.GetRoleByIdAsync(role.Id, Arg.Any<CancellationToken>()).Returns(role);
        _repository.GetByCodesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>()).Returns(new List<Permission>());
        var existing = new List<RolePermission> { new() { RoleId = role.Id, PermissionId = Guid.NewGuid() } };
        _repository.GetRolePermissionsAsync(role.Id, Arg.Any<CancellationToken>()).Returns(existing);

        var dto = await CreateService().SyncRolePermissionsAsync(
            role.Id, Guid.NewGuid(), new SyncRolePermissionsRequest { PermissionCodes = new() }, Ct);

        _repository.Received(1).RemoveRolePermissions(existing);
        Assert.Empty(dto.PermissionCodes);
    }
}
