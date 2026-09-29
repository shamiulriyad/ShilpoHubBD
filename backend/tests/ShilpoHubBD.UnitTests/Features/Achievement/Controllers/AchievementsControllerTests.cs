using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Achievement;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Achievement.Controllers;

[Trait("Feature", "Achievement")]
[Trait("Layer", "Controller")]
public class AchievementsControllerTests
{
    private readonly IAchievementService _service = Substitute.For<IAchievementService>();
    private readonly Guid _userId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private AchievementsController CreateController(params string[] roles) => new AchievementsController(_service).WithUser(_userId, roles);

    [Fact]
    public void Controller_HasNoClassLevelRoleRestrictionUnderApiAchievements()
    {
        Assert.Null(AccessRules.ClassRoles(typeof(AchievementsController)));
        Assert.False(AccessRules.ClassRequiresSignIn(typeof(AchievementsController)));
        Assert.Equal("api/achievements", AccessRules.ControllerRoute(typeof(AchievementsController)));
    }

    [Theory]
    [InlineData(nameof(AchievementsController.GetAllAchievements))]
    public void PublicActions_HaveNoRoleRestriction(string action)
        => Assert.Empty(typeof(AchievementsController).GetMethod(action)!.GetCustomAttributes(typeof(AuthorizeAttribute), false));

    [Theory]
    [InlineData(nameof(AchievementsController.GetMyXpSummary))]
    [InlineData(nameof(AchievementsController.GetMyXpHistory))]
    [InlineData(nameof(AchievementsController.GetMyAchievements))]
    [InlineData(nameof(AchievementsController.Evaluate))]
    public void SignedInActions_RequireSignInWithoutARoleRestriction(string action)
    {
        var attribute = typeof(AchievementsController).GetMethod(action)!
            .GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single();
        Assert.Null(attribute.Roles);
    }

    [Theory]
    [InlineData(nameof(AchievementsController.CreateAchievement))]
    [InlineData(nameof(AchievementsController.AwardXp))]
    public void AdminActions_RestrictToSuperAdmin(string action)
    {
        var attribute = typeof(AchievementsController).GetMethod(action)!
            .GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single();
        Assert.Equal(RoleNames.SuperAdmin, attribute.Roles);
    }

    [Theory]
    [InlineData(nameof(AchievementsController.GetMyXpSummary), "GET", "xp/mine")]
    [InlineData(nameof(AchievementsController.GetMyXpHistory), "GET", "xp/mine/history")]
    [InlineData(nameof(AchievementsController.GetAllAchievements), "GET", null)]
    [InlineData(nameof(AchievementsController.GetMyAchievements), "GET", "mine")]
    [InlineData(nameof(AchievementsController.CreateAchievement), "POST", null)]
    [InlineData(nameof(AchievementsController.AwardXp), "POST", "xp/award")]
    [InlineData(nameof(AchievementsController.Evaluate), "POST", "evaluate")]
    public void Actions_UseTheirVerbAndRoute(string action, string method, string? template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(AchievementsController), action));

    [Fact]
    public async Task GetMyXpSummary_ReturnsTheSignedInUsersSummary()
    {
        var dto = new XpSummaryDto { TotalXp = 100 };
        _service.GetMyXpSummaryAsync(_userId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().GetMyXpSummary(Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetMyXpHistory_ReturnsTheSignedInUsersHistory()
    {
        var list = new List<XpTransactionDto> { new() { Id = Guid.NewGuid() } };
        _service.GetMyXpHistoryAsync(_userId, Arg.Any<CancellationToken>()).Returns(list);

        var result = await CreateController().GetMyXpHistory(Ct);

        Assert.Same(list, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetAllAchievements_ReturnsAllAchievements()
    {
        var list = new List<AchievementDto> { new() { Id = Guid.NewGuid() } };
        _service.GetAllAchievementsAsync(Arg.Any<CancellationToken>()).Returns(list);

        var result = await CreateController().GetAllAchievements(Ct);

        Assert.Same(list, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetMyAchievements_ReturnsTheSignedInUsersAchievements()
    {
        var list = new List<UserAchievementDto> { new() { Id = Guid.NewGuid() } };
        _service.GetMyAchievementsAsync(_userId, Arg.Any<CancellationToken>()).Returns(list);

        var result = await CreateController().GetMyAchievements(Ct);

        Assert.Same(list, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task CreateAchievement_ReturnsTheCreatedAchievement()
    {
        var request = new CreateAchievementRequest { Name = "First Sale" };
        var dto = new AchievementDto { Id = Guid.NewGuid() };
        _service.CreateAchievementAsync(request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController(RoleNames.SuperAdmin).CreateAchievement(request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task AwardXp_ReturnsTheUpdatedSummary()
    {
        var request = new AwardXpRequest { UserId = Guid.NewGuid(), Amount = 10, Reason = "x" };
        var dto = new XpSummaryDto { TotalXp = 10 };
        _service.AwardXpAsync(request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController(RoleNames.SuperAdmin).AwardXp(request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Evaluate_PassesTheSignedInUser()
    {
        var list = new List<UserAchievementDto>();
        _service.EvaluateAchievementsAsync(_userId, Arg.Any<CancellationToken>()).Returns(list);

        var result = await CreateController().Evaluate(Ct);

        Assert.Same(list, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}
