using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Admin.Controllers;

[Trait("Feature", "Admin")]
[Trait("Layer", "Controller")]
public class IdentityVerificationsControllerTests
{
    private readonly IIdentityVerificationService _service = Substitute.For<IIdentityVerificationService>();
    private readonly Guid _userId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private IdentityVerificationsController CreateController(params string[] roles)
        => new IdentityVerificationsController(_service).WithUser(_userId, roles);

    [Fact]
    public void Controller_RequiresSignInAtClassLevelUnderItsOwnRoute()
    {
        Assert.True(AccessRules.ClassRequiresSignIn(typeof(IdentityVerificationsController)));
        Assert.Null(AccessRules.ClassRoles(typeof(IdentityVerificationsController)));
        Assert.Equal("api/identity-verifications", AccessRules.ControllerRoute(typeof(IdentityVerificationsController)));
    }

    [Theory]
    [InlineData(nameof(IdentityVerificationsController.Submit))]
    [InlineData(nameof(IdentityVerificationsController.GetMine))]
    public void SelfServiceActions_HaveNoExtraRoleRestriction(string action)
    {
        var method = typeof(IdentityVerificationsController).GetMethod(action)!;
        Assert.Empty(method.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false));
    }

    [Theory]
    [InlineData(nameof(IdentityVerificationsController.GetPaged))]
    [InlineData(nameof(IdentityVerificationsController.GetById))]
    [InlineData(nameof(IdentityVerificationsController.Approve))]
    [InlineData(nameof(IdentityVerificationsController.Reject))]
    public void ReviewActions_AreRestrictedToSuperAdmin(string action)
        => Assert.Equal(RoleNames.SuperAdmin,
            typeof(IdentityVerificationsController).GetMethod(action)!
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)
                .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Single().Roles);

    [Theory]
    [InlineData(nameof(IdentityVerificationsController.Submit), "POST", null)]
    [InlineData(nameof(IdentityVerificationsController.GetMine), "GET", "me")]
    [InlineData(nameof(IdentityVerificationsController.GetPaged), "GET", null)]
    [InlineData(nameof(IdentityVerificationsController.GetById), "GET", "{id:guid}")]
    [InlineData(nameof(IdentityVerificationsController.Approve), "POST", "{id:guid}/approve")]
    [InlineData(nameof(IdentityVerificationsController.Reject), "POST", "{id:guid}/reject")]
    public void Actions_UseTheirVerbAndRoute(string action, string method, string? template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(IdentityVerificationsController), action));

    [Fact]
    public async Task Submit_SubmitsForTheSignedInUserAndReturns201PointingAtGetById()
    {
        var request = new SubmitIdentityVerificationRequest { Type = "NationalId" };
        var dto = new IdentityVerificationDto { Id = Guid.NewGuid() };
        _service.SubmitAsync(_userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Submit(request, Ct);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(IdentityVerificationsController.GetById), created.ActionName);
        Assert.Equal(dto.Id, created.RouteValues?["id"]);
        Assert.Same(dto, created.Value);
    }

    [Fact]
    public async Task GetMine_ReturnsTheSignedInUsersOwnRequests()
    {
        var mine = new List<IdentityVerificationDto> { new() { Id = Guid.NewGuid(), UserId = _userId } };
        _service.GetMineAsync(_userId, Arg.Any<CancellationToken>()).Returns(mine);

        var result = await CreateController().GetMine(Ct);

        Assert.Same(mine, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetPaged_PassesTheQueryThroughAndReturnsOk()
    {
        var query = new IdentityVerificationQueryParameters { Status = "Pending" };
        var page = new PagedResult<IdentityVerificationDto>();
        _service.GetPagedAsync(query, Arg.Any<CancellationToken>()).Returns(page);

        var result = await CreateController(RoleNames.SuperAdmin).GetPaged(query, Ct);

        Assert.Same(page, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Approve_UsesTheSignedInAdminAsReviewer()
    {
        var requestId = Guid.NewGuid();
        var dto = new IdentityVerificationDto { Id = requestId, Status = "Approved" };
        _service.ApproveAsync(requestId, _userId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController(RoleNames.SuperAdmin).Approve(requestId, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Approve_AlreadyReviewed_PropagatesTheConflict()
    {
        _service.ApproveAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<IdentityVerificationDto>(_ => throw new ConflictException("This request has already been Approved."));

        await Assert.ThrowsAsync<ConflictException>(() => CreateController(RoleNames.SuperAdmin).Approve(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task Reject_PassesTheReviewerAndRequestBody()
    {
        var requestId = Guid.NewGuid();
        var request = new RejectIdentityVerificationRequest { RejectionReason = "Blurry photo." };
        var dto = new IdentityVerificationDto { Id = requestId, Status = "Rejected" };
        _service.RejectAsync(requestId, _userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController(RoleNames.SuperAdmin).Reject(requestId, request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}
