using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Admin.Controllers;

[Trait("Feature", "Admin")]
[Trait("Layer", "Controller")]
public class PermissionsControllerTests
{
    private readonly IPermissionService _service = Substitute.For<IPermissionService>();
    private readonly Guid _adminId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private PermissionsController CreateController() => new PermissionsController(_service).WithUser(_adminId, RoleNames.SuperAdmin);

    [Fact]
    public void Controller_IsSuperAdminOnly_WithNoClassLevelRoutePrefix()
    {
        Assert.Equal(RoleNames.SuperAdmin, AccessRules.ClassRoles(typeof(PermissionsController)));
        Assert.Null(AccessRules.ControllerRoute(typeof(PermissionsController)));
        Assert.All(AccessRules.PublicActions(typeof(PermissionsController)),
            action => Assert.False(AccessRules.ActionAllowsAnonymous(typeof(PermissionsController), action)));
    }

    [Theory]
    [InlineData(nameof(PermissionsController.GetAll), "GET", "api/admin/permissions")]
    [InlineData(nameof(PermissionsController.Create), "POST", "api/admin/permissions")]
    [InlineData(nameof(PermissionsController.Delete), "DELETE", "api/admin/permissions/{id:guid}")]
    [InlineData(nameof(PermissionsController.GetRoles), "GET", "api/admin/roles")]
    [InlineData(nameof(PermissionsController.GetRolePermissions), "GET", "api/admin/roles/{roleId:guid}/permissions")]
    [InlineData(nameof(PermissionsController.SyncRolePermissions), "PUT", "api/admin/roles/{roleId:guid}/permissions")]
    public void Actions_UseTheirVerbAndFullRoute(string action, string method, string template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(PermissionsController), action));

    [Fact]
    public async Task GetAll_ReturnsEveryPermission()
    {
        var permissions = new List<PermissionDto> { new() { Code = "users.manage" } };
        _service.GetAllAsync(Arg.Any<CancellationToken>()).Returns(permissions);

        var result = await CreateController().GetAll(Ct);

        Assert.Same(permissions, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Create_CreatesThePermissionAndReturns201()
    {
        var request = new CreatePermissionRequest { Code = "x", Name = "x", Module = "x" };
        var dto = new PermissionDto { Id = Guid.NewGuid(), Code = "x" };
        _service.CreateAsync(request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Create(request, Ct);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(PermissionsController.GetAll), created.ActionName);
        Assert.Same(dto, created.Value);
    }

    [Fact]
    public async Task Create_DuplicateCode_PropagatesTheConflict()
    {
        _service.CreateAsync(Arg.Any<CreatePermissionRequest>(), Arg.Any<CancellationToken>())
            .Returns<PermissionDto>(_ => throw new ConflictException("A permission with this code already exists."));

        await Assert.ThrowsAsync<ConflictException>(
            () => CreateController().Create(new CreatePermissionRequest { Code = "x", Name = "x", Module = "x" }, Ct));
    }

    [Fact]
    public async Task Delete_DeletesThePermissionAndReturnsNoContent()
    {
        var id = Guid.NewGuid();

        var result = await CreateController().Delete(id, Ct);

        Assert.IsType<NoContentResult>(result);
        await _service.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetRoles_ReturnsTheRoleList()
    {
        var roles = new List<RoleAdminDto> { new() { Name = "Producer" } };
        _service.GetRolesAsync(Arg.Any<CancellationToken>()).Returns(roles);

        var result = await CreateController().GetRoles(Ct);

        Assert.Same(roles, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetRolePermissions_ReturnsThatRolesGrants()
    {
        var roleId = Guid.NewGuid();
        var dto = new RolePermissionsDto { RoleId = roleId };
        _service.GetRolePermissionsAsync(roleId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().GetRolePermissions(roleId, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task SyncRolePermissions_UsesTheSignedInAdminAsGranter()
    {
        var roleId = Guid.NewGuid();
        var request = new SyncRolePermissionsRequest { PermissionCodes = new() { "users.manage" } };
        var dto = new RolePermissionsDto { RoleId = roleId };
        _service.SyncRolePermissionsAsync(roleId, _adminId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().SyncRolePermissions(roleId, request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}
