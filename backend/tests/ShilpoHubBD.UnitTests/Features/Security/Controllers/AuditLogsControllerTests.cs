using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Security.Controllers;

[Trait("Feature", "Security")]
[Trait("Layer", "Controller")]
public class AuditLogsControllerTests
{
    private readonly IAuditLogService _service = Substitute.For<IAuditLogService>();

    [Fact]
    public void Controller_IsSuperAdminOnlyUnderTheSecurityAuditLogsRoute()
    {
        Assert.Equal(RoleNames.SuperAdmin, AccessRules.ClassRoles(typeof(AuditLogsController)));
        Assert.Equal("api/admin/security/audit-logs", AccessRules.ControllerRoute(typeof(AuditLogsController)));
        Assert.Equal(("GET", (string?)null), AccessRules.ActionRoute(typeof(AuditLogsController), nameof(AuditLogsController.GetPaged)));
        Assert.False(AccessRules.ActionAllowsAnonymous(typeof(AuditLogsController), nameof(AuditLogsController.GetPaged)));
    }

    [Fact]
    public void Controller_OnlyReadsLogs_ItHasNoWriteActions()
        => Assert.Equal(new[] { nameof(AuditLogsController.GetPaged) }, AccessRules.PublicActions(typeof(AuditLogsController)));

    [Fact]
    public async Task GetPaged_PassesTheQueryThroughAndReturnsOk()
    {
        var query = new AuditLogQueryParameters { Action = "Product.Approved", Search = "saree", Page = 2 };
        var page = new PagedResult<AuditLogDto> { Page = 2 };
        _service.GetPagedAsync(query, Arg.Any<CancellationToken>()).Returns(page);
        var controller = new AuditLogsController(_service).WithUser(Guid.NewGuid(), RoleNames.SuperAdmin);

        var result = await controller.GetPaged(query, TestContext.Current.CancellationToken);

        Assert.Same(page, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}
