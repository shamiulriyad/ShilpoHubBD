using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Mentorship;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Mentorship.Controllers;

[Trait("Feature", "Mentorship")]
[Trait("Layer", "Controller")]
public class MentorshipRequestsControllerTests
{
    private readonly IMentorshipService _service = Substitute.For<IMentorshipService>();
    private readonly Guid _userId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private MentorshipRequestsController CreateController() => new MentorshipRequestsController(_service).WithUser(_userId);

    [Fact]
    public void Controller_RequiresSignInAtTheClassLevelWithNoRoleRestriction()
    {
        Assert.True(AccessRules.ClassRequiresSignIn(typeof(MentorshipRequestsController)));
        Assert.Null(AccessRules.ClassRoles(typeof(MentorshipRequestsController)));
        Assert.Equal("api/mentorship-requests", AccessRules.ControllerRoute(typeof(MentorshipRequestsController)));
    }

    [Theory]
    [InlineData(nameof(MentorshipRequestsController.Create), "POST", null)]
    [InlineData(nameof(MentorshipRequestsController.GetById), "GET", "{id:guid}")]
    [InlineData(nameof(MentorshipRequestsController.GetMineAsLearner), "GET", "mine/as-learner")]
    [InlineData(nameof(MentorshipRequestsController.GetMineAsMentor), "GET", "mine/as-mentor")]
    [InlineData(nameof(MentorshipRequestsController.Accept), "POST", "{id:guid}/accept")]
    [InlineData(nameof(MentorshipRequestsController.Reject), "POST", "{id:guid}/reject")]
    [InlineData(nameof(MentorshipRequestsController.Complete), "POST", "{id:guid}/complete")]
    public void Actions_UseTheirVerbAndRoute(string action, string method, string? template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(MentorshipRequestsController), action));

    [Fact]
    public async Task Create_UsesTheSignedInLearnerAndReturns201PointingAtGetById()
    {
        var request = new CreateMentorshipRequestRequest { MentorProfileId = Guid.NewGuid(), Message = "x" };
        var dto = new MentorshipRequestDto { Id = Guid.NewGuid() };
        _service.CreateRequestAsync(_userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Create(request, Ct);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(MentorshipRequestsController.GetById), created.ActionName);
        Assert.Equal(dto.Id, created.RouteValues?["id"]);
        Assert.Same(dto, created.Value);
    }

    [Fact]
    public async Task GetById_UsesTheSignedInUser()
    {
        var id = Guid.NewGuid();
        var dto = new MentorshipRequestDto { Id = id };
        _service.GetByIdAsync(_userId, id, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().GetById(id, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetMineAsLearner_ReturnsTheSignedInUsersRequests()
    {
        var list = new List<MentorshipRequestListItemDto> { new() { Id = Guid.NewGuid() } };
        _service.GetMyRequestsAsLearnerAsync(_userId, Arg.Any<CancellationToken>()).Returns(list);

        var result = await CreateController().GetMineAsLearner(Ct);

        Assert.Same(list, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetMineAsMentor_ReturnsTheSignedInUsersRequests()
    {
        var list = new List<MentorshipRequestListItemDto> { new() { Id = Guid.NewGuid() } };
        _service.GetMyRequestsAsMentorAsync(_userId, Arg.Any<CancellationToken>()).Returns(list);

        var result = await CreateController().GetMineAsMentor(Ct);

        Assert.Same(list, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Accept_PassesTheSignedInMentorAndRequestThrough()
    {
        var id = Guid.NewGuid();
        var request = new RespondMentorshipRequestRequest { ResponseMessage = "Welcome" };
        var dto = new MentorshipRequestDto { Id = id };
        _service.AcceptAsync(_userId, id, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Accept(id, request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Reject_PassesTheSignedInMentorAndRequestThrough()
    {
        var id = Guid.NewGuid();
        var request = new RespondMentorshipRequestRequest();
        var dto = new MentorshipRequestDto { Id = id };
        _service.RejectAsync(_userId, id, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Reject(id, request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Complete_PassesTheSignedInMentor()
    {
        var id = Guid.NewGuid();
        var dto = new MentorshipRequestDto { Id = id };
        _service.CompleteAsync(_userId, id, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Complete(id, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}
