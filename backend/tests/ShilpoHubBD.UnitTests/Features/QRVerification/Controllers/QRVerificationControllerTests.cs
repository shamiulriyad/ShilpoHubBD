using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.QRVerification;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.QRVerification.Controllers;

[Trait("Feature", "QRVerification")]
[Trait("Layer", "Controller")]
public class QRVerificationControllerTests
{
    private readonly IQRVerificationService _service = Substitute.For<IQRVerificationService>();
    private readonly Guid _userId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private QRVerificationController CreateController(params string[] roles) => new QRVerificationController(_service).WithUser(_userId, roles);

    [Fact]
    public void Controller_HasNoClassLevelRoleRestrictionUnderApiQrVerification()
    {
        Assert.Null(AccessRules.ClassRoles(typeof(QRVerificationController)));
        Assert.False(AccessRules.ClassRequiresSignIn(typeof(QRVerificationController)));
        Assert.Equal("api/qr-verification", AccessRules.ControllerRoute(typeof(QRVerificationController)));
    }

    [Fact]
    public void Verify_HasNoRoleRestriction()
        => Assert.Empty(typeof(QRVerificationController).GetMethod(nameof(QRVerificationController.Verify))!
            .GetCustomAttributes(typeof(AuthorizeAttribute), false));

    [Fact]
    public void GetMyHistory_RequiresSignInWithoutARoleRestriction()
    {
        var attribute = typeof(QRVerificationController).GetMethod(nameof(QRVerificationController.GetMyHistory))!
            .GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single();
        Assert.Null(attribute.Roles);
    }

    [Theory]
    [InlineData(nameof(QRVerificationController.Generate))]
    [InlineData(nameof(QRVerificationController.Revoke))]
    [InlineData(nameof(QRVerificationController.GetProductHistory))]
    public void ProducerFacingActions_RestrictToProducerOrSuperAdmin(string action)
    {
        var attribute = typeof(QRVerificationController).GetMethod(action)!
            .GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single();
        Assert.Equal($"{RoleNames.Producer},{RoleNames.SuperAdmin}", attribute.Roles);
    }

    [Theory]
    [InlineData(nameof(QRVerificationController.Verify), "POST", "verify")]
    [InlineData(nameof(QRVerificationController.Generate), "POST", "generate")]
    [InlineData(nameof(QRVerificationController.Revoke), "POST", "{id:guid}/revoke")]
    [InlineData(nameof(QRVerificationController.GetMyHistory), "GET", "history/mine")]
    [InlineData(nameof(QRVerificationController.GetProductHistory), "GET", "products/{productId:guid}/history")]
    public void Actions_UseTheirVerbAndRoute(string action, string method, string? template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(QRVerificationController), action));

    [Fact]
    public async Task Verify_AnonymousCaller_PassesNullUserId()
    {
        var request = new VerifyQRRequest { Code = "abc" };
        var dto = new QRVerificationResultDto { IsValid = true };
        _service.VerifyAsync(null, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await new QRVerificationController(_service).WithAnonymousRequest().Verify(request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Verify_SignedInCaller_PassesTheirUserId()
    {
        var request = new VerifyQRRequest { Code = "abc" };
        var dto = new QRVerificationResultDto { IsValid = true };
        _service.VerifyAsync(_userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Verify(request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Generate_UsesTheSignedInProducer()
    {
        var request = new GenerateQRCodeRequest { ProductId = Guid.NewGuid() };
        var dto = new QRCodeDto { Id = Guid.NewGuid() };
        _service.GenerateAsync(_userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController(RoleNames.Producer).Generate(request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Revoke_PassesTheSignedInUserAndWhetherTheyAreAdmin()
    {
        var id = Guid.NewGuid();

        var result = await CreateController(RoleNames.SuperAdmin).Revoke(id, Ct);

        Assert.IsType<NoContentResult>(result);
        await _service.Received(1).RevokeAsync(id, _userId, true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Revoke_Producer_PassesIsAdminFalse()
    {
        var id = Guid.NewGuid();

        await CreateController(RoleNames.Producer).Revoke(id, Ct);

        await _service.Received(1).RevokeAsync(id, _userId, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMyHistory_ReturnsTheSignedInUsersHistory()
    {
        var query = new QRVerificationQueryParameters();
        var page = new PagedResult<QRVerificationHistoryItemDto>();
        _service.GetMyHistoryAsync(_userId, query, Arg.Any<CancellationToken>()).Returns(page);

        var result = await CreateController().GetMyHistory(query, Ct);

        Assert.Same(page, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetProductHistory_PassesTheSignedInProducerAndWhetherTheyAreAdmin()
    {
        var productId = Guid.NewGuid();
        var query = new QRVerificationQueryParameters();
        var page = new PagedResult<QRVerificationHistoryItemDto>();
        _service.GetProductHistoryAsync(productId, _userId, true, query, Arg.Any<CancellationToken>()).Returns(page);

        var result = await CreateController(RoleNames.SuperAdmin).GetProductHistory(productId, query, Ct);

        Assert.Same(page, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}
