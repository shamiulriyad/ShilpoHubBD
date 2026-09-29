using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.MentorMatching;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.MentorMatching.Controllers;

[Trait("Feature", "MentorMatching")]
[Trait("Layer", "Controller")]
public class MentorMatchingControllerTests
{
    private readonly IMentorMatchingService _service = Substitute.For<IMentorMatchingService>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public void Controller_RequiresSignInAtTheClassLevelWithNoRoleRestriction()
    {
        Assert.True(AccessRules.ClassRequiresSignIn(typeof(MentorMatchingController)));
        Assert.Null(AccessRules.ClassRoles(typeof(MentorMatchingController)));
        Assert.Equal("api/mentor-matching", AccessRules.ControllerRoute(typeof(MentorMatchingController)));
    }

    [Fact]
    public void Match_UsesPostVerbAndItsRoute()
        => Assert.Equal(("POST", "match"), AccessRules.ActionRoute(typeof(MentorMatchingController), nameof(MentorMatchingController.Match)));

    [Fact]
    public async Task Match_ReturnsTheServicesResult()
    {
        var request = new MentorMatchRequest();
        var list = new List<MentorMatchResultDto> { new() { MentorProfileId = Guid.NewGuid() } };
        _service.MatchAsync(request, Arg.Any<CancellationToken>()).Returns(list);

        var result = await new MentorMatchingController(_service).WithUser(Guid.NewGuid()).Match(request, Ct);

        Assert.Same(list, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}
