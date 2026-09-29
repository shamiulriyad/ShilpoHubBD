using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Admin.Controllers;

[Trait("Feature", "Admin")]
[Trait("Layer", "Controller")]
public class AdminUsersControllerTests
{
    private const string Ip = "203.0.113.11";
    private readonly IAdminUserService _service = Substitute.For<IAdminUserService>();
    private readonly Guid _adminId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private AdminUsersController CreateController()
        => new AdminUsersController(_service).WithUser(_adminId, RoleNames.SuperAdmin).WithRemoteIp(Ip);

    [Fact]
    public void Controller_IsSuperAdminOnlyUnderTheAdminUsersRoute()
    {
        Assert.Equal(RoleNames.SuperAdmin, AccessRules.ClassRoles(typeof(AdminUsersController)));
        Assert.Equal("api/admin/users", AccessRules.ControllerRoute(typeof(AdminUsersController)));
        Assert.All(AccessRules.PublicActions(typeof(AdminUsersController)),
            action => Assert.False(AccessRules.ActionAllowsAnonymous(typeof(AdminUsersController), action)));
    }

    [Theory]
    [InlineData(nameof(AdminUsersController.GetPaged), "GET", null)]
    [InlineData(nameof(AdminUsersController.GetById), "GET", "{id:guid}")]
    [InlineData(nameof(AdminUsersController.Activate), "POST", "{id:guid}/activate")]
    [InlineData(nameof(AdminUsersController.Deactivate), "POST", "{id:guid}/deactivate")]
    public void Actions_UseTheirVerbAndRoute(string action, string method, string? template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(AdminUsersController), action));

    [Fact]
    public async Task GetPaged_PassesTheQueryThroughAndReturnsOk()
    {
        var query = new AdminUserQueryParameters { Search = "rahima" };
        var page = new PagedResult<AdminUserListItemDto> { Page = 1 };
        _service.GetPagedAsync(query, Arg.Any<CancellationToken>()).Returns(page);

        var result = await CreateController().GetPaged(query, Ct);

        Assert.Same(page, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetById_ReturnsTheDetail()
    {
        var dto = new AdminUserDetailDto { Id = Guid.NewGuid() };
        _service.GetByIdAsync(dto.Id, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().GetById(dto.Id, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetById_Unknown_PropagatesNotFound()
    {
        _service.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<AdminUserDetailDto>(_ => throw new NotFoundException("User not found."));

        await Assert.ThrowsAsync<NotFoundException>(() => CreateController().GetById(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task Activate_PassesTheSignedInAdminAndClientIp()
    {
        var targetId = Guid.NewGuid();
        var dto = new AdminUserDetailDto { Id = targetId, IsActive = true };
        _service.SetActiveAsync(targetId, true, _adminId, Ip, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Activate(targetId, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Deactivate_PassesTheSignedInAdminAndClientIp()
    {
        var targetId = Guid.NewGuid();
        var dto = new AdminUserDetailDto { Id = targetId, IsActive = false };
        _service.SetActiveAsync(targetId, false, _adminId, Ip, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Deactivate(targetId, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}
