using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Security.Controllers;

[Trait("Feature", "Security")]
[Trait("Layer", "Controller")]
public class SystemHealthControllerTests
{
    private readonly ISystemHealthService _service = Substitute.For<ISystemHealthService>();

    [Fact]
    public void Controller_IsSuperAdminOnlyUnderTheSecuritySystemHealthRoute()
    {
        Assert.Equal(RoleNames.SuperAdmin, AccessRules.ClassRoles(typeof(SystemHealthController)));
        Assert.Equal("api/admin/security/system-health", AccessRules.ControllerRoute(typeof(SystemHealthController)));
        Assert.Equal(("GET", (string?)null), AccessRules.ActionRoute(typeof(SystemHealthController), nameof(SystemHealthController.Get)));
        Assert.False(AccessRules.ActionAllowsAnonymous(typeof(SystemHealthController), nameof(SystemHealthController.Get)));
    }

    [Fact]
    public async Task Get_ReturnsTheHealthReport()
    {
        var health = new SystemHealthDto { DatabaseConnected = true, UserCount = 3 };
        _service.GetHealthAsync(Arg.Any<CancellationToken>()).Returns(health);
        var controller = new SystemHealthController(_service).WithUser(Guid.NewGuid(), RoleNames.SuperAdmin);

        var result = await controller.Get(TestContext.Current.CancellationToken);

        Assert.Same(health, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}
