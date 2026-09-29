using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Recommendation.Controllers;

[Trait("Feature", "Recommendation")]
[Trait("Layer", "Controller")]
public class RecommendationsControllerTests
{
    private readonly IRecommendationService _service = Substitute.For<IRecommendationService>();
    private readonly Guid _userId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public void Controller_HasNoClassLevelRoleRestrictionUnderApiRecommendations()
    {
        Assert.Null(AccessRules.ClassRoles(typeof(RecommendationsController)));
        Assert.False(AccessRules.ClassRequiresSignIn(typeof(RecommendationsController)));
        Assert.Equal("api/recommendations", AccessRules.ControllerRoute(typeof(RecommendationsController)));
    }

    [Theory]
    [InlineData(nameof(RecommendationsController.GetForMe), "GET", null)]
    [InlineData(nameof(RecommendationsController.GetSimilar), "GET", "similar/{productId:guid}")]
    public void Actions_UseTheirVerbAndRoute(string action, string method, string? template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(RecommendationsController), action));

    [Fact]
    public async Task GetForMe_SignedInCaller_PassesTheirUserId()
    {
        var list = new List<ProductListItemDto> { new() { Id = Guid.NewGuid() } };
        _service.GetRecommendedForMeAsync(_userId, 8, Arg.Any<CancellationToken>()).Returns(list);

        var result = await new RecommendationsController(_service).WithUser(_userId).GetForMe(Ct);

        Assert.Same(list, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetForMe_AnonymousCaller_PassesNullUserId()
    {
        var list = new List<ProductListItemDto>();
        _service.GetRecommendedForMeAsync(null, 8, Arg.Any<CancellationToken>()).Returns(list);

        var result = await new RecommendationsController(_service).WithAnonymousRequest().GetForMe(Ct);

        Assert.Same(list, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetForMe_UsesTheGivenCount()
    {
        _service.GetRecommendedForMeAsync(Arg.Any<Guid?>(), 3, Arg.Any<CancellationToken>()).Returns(new List<ProductListItemDto>());

        await new RecommendationsController(_service).WithAnonymousRequest().GetForMe(Ct, count: 3);

        await _service.Received(1).GetRecommendedForMeAsync(Arg.Any<Guid?>(), 3, Ct);
    }

    [Fact]
    public async Task GetSimilar_ReturnsTheServicesResult()
    {
        var productId = Guid.NewGuid();
        var list = new List<ProductListItemDto> { new() { Id = Guid.NewGuid() } };
        _service.GetSimilarAsync(productId, 8, Arg.Any<CancellationToken>()).Returns(list);

        var result = await new RecommendationsController(_service).WithAnonymousRequest().GetSimilar(productId, Ct);

        Assert.Same(list, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}
