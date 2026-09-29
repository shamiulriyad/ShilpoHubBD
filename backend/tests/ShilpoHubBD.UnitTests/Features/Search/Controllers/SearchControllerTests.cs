using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Search.Controllers;

[Trait("Feature", "Search")]
[Trait("Layer", "Controller")]
public class SearchControllerTests
{
    private readonly ISearchService _service = Substitute.For<ISearchService>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private SearchController CreateController() => new SearchController(_service).WithAnonymousRequest();

    [Fact]
    public void Controller_IsPublicUnderApiSearchWithTheReadRateLimit()
    {
        Assert.False(AccessRules.ClassRequiresSignIn(typeof(SearchController)));
        Assert.Equal("api/search", AccessRules.ControllerRoute(typeof(SearchController)));
        Assert.Equal("read", typeof(SearchController).GetCustomAttribute<EnableRateLimitingAttribute>()?.PolicyName);
    }

    [Fact]
    public void Search_IsAGetOnTheControllerRoute()
        => Assert.Equal(("GET", (string?)null), AccessRules.ActionRoute(typeof(SearchController), nameof(SearchController.Search)));

    [Fact]
    public async Task Search_PassesTheQueryPageAndPageSizeThrough()
    {
        var page = new PagedResult<ProductListItemDto> { Page = 2, PageSize = 24 };
        _service.SearchAsync("jamdani", 2, 24, Arg.Any<CancellationToken>()).Returns(page);

        var result = await CreateController().Search("jamdani", Ct, 2, 24);

        Assert.Same(page, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Search_Defaults_AreFirstPageOfTwelve()
    {
        _service.SearchAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<ProductListItemDto>());

        await CreateController().Search("saree", Ct);

        await _service.Received(1).SearchAsync("saree", 1, 12, Arg.Any<CancellationToken>());
    }
}
