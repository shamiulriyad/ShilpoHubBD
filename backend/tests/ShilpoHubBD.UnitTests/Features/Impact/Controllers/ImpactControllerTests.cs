using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Impact;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Impact.Controllers;

[Trait("Feature", "Impact")]
[Trait("Layer", "Controller")]
public class ImpactControllerTests
{
    private readonly IImpactService _service = Substitute.For<IImpactService>();
    private readonly Guid _userId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private ImpactController CreateController() => new ImpactController(_service).WithUser(_userId);

    [Fact]
    public void Controller_RequiresSignInAtTheClassLevelWithNoRoleRestriction()
    {
        Assert.True(AccessRules.ClassRequiresSignIn(typeof(ImpactController)));
        Assert.Null(AccessRules.ClassRoles(typeof(ImpactController)));
        Assert.Equal("api/impact", AccessRules.ControllerRoute(typeof(ImpactController)));
    }

    [Fact]
    public void GetMyImpact_UsesGetVerbAndMineRoute()
        => Assert.Equal(("GET", "mine"), AccessRules.ActionRoute(typeof(ImpactController), nameof(ImpactController.GetMyImpact)));

    [Fact]
    public async Task GetMyImpact_ReturnsTheSignedInUsersSummary()
    {
        var dto = new ImpactSummaryDto { HeritageScore = 45 };
        _service.GetMyImpactAsync(_userId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().GetMyImpact(Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}
