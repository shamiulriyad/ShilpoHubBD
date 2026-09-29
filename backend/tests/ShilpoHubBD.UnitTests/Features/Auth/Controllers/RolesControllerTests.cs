using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Roles;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Auth.Controllers;

[Trait("Feature", "Auth")]
[Trait("Layer", "Controller")]
public class RolesControllerTests
{
    private readonly IRoleService _service = Substitute.For<IRoleService>();
    private readonly Guid _adminId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private RolesController CreateController() => new RolesController(_service).WithUser(_adminId, RoleNames.SuperAdmin);

    [Fact]
    public void Controller_IsRestrictedToSuperAdminUnderApiRoles()
    {
        Assert.Equal(RoleNames.SuperAdmin, AccessRules.ClassRoles(typeof(RolesController)));
        Assert.Equal("api/roles", AccessRules.ControllerRoute(typeof(RolesController)));
    }

    [Theory]
    [InlineData(nameof(RolesController.AssignRole), "assign")]
    [InlineData(nameof(RolesController.RemoveRole), "remove")]
    public void Actions_ArePostsOnTheirRoutes(string action, string template)
        => Assert.Equal(("POST", template), AccessRules.ActionRoute(typeof(RolesController), action));

    [Theory]
    [InlineData(nameof(RolesController.AssignRole))]
    [InlineData(nameof(RolesController.RemoveRole))]
    public void Actions_DoNotOpenUpAnonymousAccess(string action)
        => Assert.False(AccessRules.ActionAllowsAnonymous(typeof(RolesController), action));

    [Fact]
    public async Task AssignRole_RecordsTheSignedInAdminAsAssignerAndReturnsSuccess()
    {
        var targetUser = Guid.NewGuid();

        var result = await CreateController().AssignRole(new AssignRoleRequest { UserId = targetUser, Role = RoleNames.Producer }, Ct);

        var body = Assert.IsType<MessageResponse>(Assert.IsType<OkObjectResult>(result).Value);
        Assert.True(body.Success);
        Assert.Equal("Role assigned successfully.", body.Message);
        await _service.Received(1).AssignRoleAsync(targetUser, RoleNames.Producer, _adminId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignRole_AdminIdentifiedByNameIdentifierClaim_StillRecordsTheAdmin()
    {
        var controller = new RolesController(_service).WithNameIdentifierUser(_adminId, RoleNames.SuperAdmin);

        await controller.AssignRole(new AssignRoleRequest { UserId = Guid.NewGuid(), Role = RoleNames.Tourist }, Ct);

        await _service.Received(1).AssignRoleAsync(Arg.Any<Guid>(), RoleNames.Tourist, _adminId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignRole_ServiceConflict_PropagatesToTheExceptionHandler()
    {
        _service.AssignRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new ConflictException("User already has this role."));

        await Assert.ThrowsAsync<ConflictException>(
            () => CreateController().AssignRole(new AssignRoleRequest { UserId = Guid.NewGuid(), Role = RoleNames.Producer }, Ct));
    }

    [Fact]
    public async Task RemoveRole_CallsTheServiceAndReturnsSuccess()
    {
        var targetUser = Guid.NewGuid();

        var result = await CreateController().RemoveRole(new RemoveRoleRequest { UserId = targetUser, Role = RoleNames.Producer }, Ct);

        var body = Assert.IsType<MessageResponse>(Assert.IsType<OkObjectResult>(result).Value);
        Assert.True(body.Success);
        Assert.Equal("Role removed successfully.", body.Message);
        await _service.Received(1).RemoveRoleAsync(targetUser, RoleNames.Producer, Arg.Any<CancellationToken>());
    }
}
