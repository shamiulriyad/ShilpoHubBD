using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Security.Controllers;

[Trait("Feature", "Security")]
[Trait("Layer", "Controller")]
public class ThreatDetectionControllerTests
{
    private readonly IThreatDetectionService _service = Substitute.For<IThreatDetectionService>();
    private readonly Guid _adminId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private ThreatDetectionController CreateController() => new ThreatDetectionController(_service).WithUser(_adminId, RoleNames.SuperAdmin);

    [Fact]
    public void Controller_IsSuperAdminOnlyUnderTheSecurityThreatsRoute()
    {
        Assert.Equal(RoleNames.SuperAdmin, AccessRules.ClassRoles(typeof(ThreatDetectionController)));
        Assert.Equal("api/admin/security/threats", AccessRules.ControllerRoute(typeof(ThreatDetectionController)));
        Assert.All(AccessRules.PublicActions(typeof(ThreatDetectionController)),
            action => Assert.False(AccessRules.ActionAllowsAnonymous(typeof(ThreatDetectionController), action)));
    }

    [Theory]
    [InlineData(nameof(ThreatDetectionController.GetFailedLogins), "GET", "failed-logins")]
    [InlineData(nameof(ThreatDetectionController.GetSuspiciousIps), "GET", "suspicious-ips")]
    [InlineData(nameof(ThreatDetectionController.GetBlockedIps), "GET", "blocked-ips")]
    [InlineData(nameof(ThreatDetectionController.BlockIp), "POST", "blocked-ips")]
    [InlineData(nameof(ThreatDetectionController.UnblockIp), "DELETE", "blocked-ips")]
    public void Actions_UseTheirVerbAndRoute(string action, string method, string template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(ThreatDetectionController), action));

    [Fact]
    public async Task GetFailedLogins_PassesPagingThroughAndReturnsOk()
    {
        var page = new PagedResult<LoginAttemptDto> { Page = 2, PageSize = 50 };
        _service.GetFailedLoginsAsync(2, 50, Arg.Any<CancellationToken>()).Returns(page);

        var result = await CreateController().GetFailedLogins(2, 50, Ct);

        Assert.Same(page, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetFailedLogins_Defaults_AreFirstPageOfTwenty()
    {
        await CreateController().GetFailedLogins(cancellationToken: Ct);

        await _service.Received(1).GetFailedLoginsAsync(1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetSuspiciousIps_ReturnsTheList()
    {
        var ips = new List<SuspiciousIpDto> { new() { IpAddress = "203.0.113.1", FailedAttempts = 9 } };
        _service.GetSuspiciousIpsAsync(Arg.Any<CancellationToken>()).Returns(ips);

        var result = await CreateController().GetSuspiciousIps(Ct);

        Assert.Same(ips, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetBlockedIps_ReturnsTheList()
    {
        var blocked = new List<BlockedIpDto> { new() { IpAddress = "203.0.113.2" } };
        _service.GetBlockedIpsAsync(Arg.Any<CancellationToken>()).Returns(blocked);

        var result = await CreateController().GetBlockedIps(Ct);

        Assert.Same(blocked, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task BlockIp_BlocksOnBehalfOfTheSignedInAdmin()
    {
        var request = new BlockIpRequest { IpAddress = "203.0.113.3", Reason = "Brute force" };
        var dto = new BlockedIpDto { IpAddress = "203.0.113.3" };
        _service.BlockIpAsync(_adminId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().BlockIp(request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task BlockIp_AlreadyBlocked_PropagatesTheConflict()
    {
        _service.BlockIpAsync(Arg.Any<Guid>(), Arg.Any<BlockIpRequest>(), Arg.Any<CancellationToken>())
            .Returns<BlockedIpDto>(_ => throw new ConflictException("'203.0.113.3' is already blocked."));

        await Assert.ThrowsAsync<ConflictException>(() => CreateController().BlockIp(new BlockIpRequest(), Ct));
    }

    [Fact]
    public async Task UnblockIp_UnblocksTheQueriedAddressAndReturnsNoContent()
    {
        var result = await CreateController().UnblockIp("203.0.113.4", Ct);

        Assert.IsType<NoContentResult>(result);
        await _service.Received(1).UnblockIpAsync("203.0.113.4", Arg.Any<CancellationToken>());
    }
}
