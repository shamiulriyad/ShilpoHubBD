using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.CounterfeitDetection;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.CounterfeitDetection.Controllers;

[Trait("Feature", "CounterfeitDetection")]
[Trait("Layer", "Controller")]
public class CounterfeitDetectionControllerTests
{
    private readonly ICounterfeitDetectionService _service = Substitute.For<ICounterfeitDetectionService>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public void Controller_HasNoClassLevelRoleRestrictionUnderApiAiCounterfeitDetection()
    {
        Assert.Null(AccessRules.ClassRoles(typeof(CounterfeitDetectionController)));
        Assert.False(AccessRules.ClassRequiresSignIn(typeof(CounterfeitDetectionController)));
        Assert.Equal("api/ai/counterfeit-detection", AccessRules.ControllerRoute(typeof(CounterfeitDetectionController)));
    }

    [Fact]
    public void Check_UsesGetVerbAndItsRoute()
        => Assert.Equal(("GET", "check/{productId:guid}"), AccessRules.ActionRoute(typeof(CounterfeitDetectionController), nameof(CounterfeitDetectionController.Check)));

    [Fact]
    public async Task Check_ReturnsTheServicesResult()
    {
        var productId = Guid.NewGuid();
        var dto = new CounterfeitCheckResultDto { ProductId = productId, RiskLevel = "Low" };
        _service.CheckAsync(productId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await new CounterfeitDetectionController(_service).WithAnonymousRequest().Check(productId, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}
