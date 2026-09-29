using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Auth;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Auth.Controllers;

[Trait("Feature", "Auth")]
[Trait("Layer", "Controller")]
public class AuthControllerTests
{
    private const string Ip = "198.51.100.23";
    private readonly IAuthService _service = Substitute.For<IAuthService>();
    private readonly AuthResponse _response = new() { AccessToken = "access", RefreshToken = "refresh", UserId = Guid.NewGuid() };

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private AuthController CreateController() => new AuthController(_service).WithAnonymousRequest().WithRemoteIp(Ip);

    // ---------- access rules and routes ----------

    [Fact]
    public void Controller_IsPublicAtClassLevelUnderApiAuthWithTheAuthRateLimit()
    {
        Assert.False(AccessRules.ClassRequiresSignIn(typeof(AuthController)));
        Assert.Equal("api/auth", AccessRules.ControllerRoute(typeof(AuthController)));
        Assert.Equal("auth", typeof(AuthController).GetCustomAttribute<EnableRateLimitingAttribute>()?.PolicyName);
    }

    [Theory]
    [InlineData(nameof(AuthController.Register))]
    [InlineData(nameof(AuthController.Login))]
    [InlineData(nameof(AuthController.Refresh))]
    [InlineData(nameof(AuthController.ForgotPassword))]
    [InlineData(nameof(AuthController.ResetPassword))]
    public void SignedOutEndpoints_DoNotRequireSignIn(string action)
        => Assert.False(AccessRules.ActionRequiresSignIn(typeof(AuthController), action));

    [Theory]
    [InlineData(nameof(AuthController.Logout))]
    [InlineData(nameof(AuthController.SwitchRole))]
    public void SessionEndpoints_RequireSignIn(string action)
        => Assert.True(AccessRules.ActionRequiresSignIn(typeof(AuthController), action));

    [Theory]
    [InlineData(nameof(AuthController.Register), "register")]
    [InlineData(nameof(AuthController.Login), "login")]
    [InlineData(nameof(AuthController.Refresh), "refresh")]
    [InlineData(nameof(AuthController.Logout), "logout")]
    [InlineData(nameof(AuthController.ForgotPassword), "forgot-password")]
    [InlineData(nameof(AuthController.ResetPassword), "reset-password")]
    [InlineData(nameof(AuthController.SwitchRole), "switch-role")]
    public void Actions_ArePostsOnTheirRoutes(string action, string template)
        => Assert.Equal(("POST", template), AccessRules.ActionRoute(typeof(AuthController), action));

    [Fact]
    public void Controller_ExposesOnlyTheKnownActions()
        => Assert.Equal(
            new[] { "Register", "Login", "Refresh", "Logout", "ForgotPassword", "ResetPassword", "SwitchRole" }.Order(),
            AccessRules.PublicActions(typeof(AuthController)).Order());

    // ---------- actions ----------

    [Fact]
    public async Task Register_PassesTheRequestAndClientIpAndReturnsOk()
    {
        var request = new RegisterRequest { Email = "new@example.com" };
        _service.RegisterAsync(request, Ip, Arg.Any<CancellationToken>()).Returns(_response);

        var result = await CreateController().Register(request, Ct);

        Assert.Same(_response, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Register_WithoutARemoteAddress_PassesANullIp()
    {
        var request = new RegisterRequest();
        var controller = new AuthController(_service).WithAnonymousRequest();

        await controller.Register(request, Ct);

        await _service.Received(1).RegisterAsync(request, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Login_PassesTheRequestAndClientIpAndReturnsOk()
    {
        var request = new LoginRequest { Email = "artisan@example.com", Password = "x" };
        _service.LoginAsync(request, Ip, Arg.Any<CancellationToken>()).Returns(_response);

        var result = await CreateController().Login(request, Ct);

        Assert.Same(_response, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Login_ServiceRejectsCredentials_TheErrorPropagatesToTheExceptionHandler()
    {
        _service.LoginAsync(Arg.Any<LoginRequest>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<AuthResponse>(_ => throw new UnauthorizedAccessException("Invalid email or password."));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateController().Login(new LoginRequest(), Ct));
    }

    [Fact]
    public async Task Refresh_PassesTheRefreshTokenAndClientIpAndReturnsOk()
    {
        _service.RefreshTokenAsync("refresh-raw", Ip, Arg.Any<CancellationToken>()).Returns(_response);

        var result = await CreateController().Refresh(new RefreshTokenRequest { RefreshToken = "refresh-raw" }, Ct);

        Assert.Same(_response, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Logout_RevokesTheGivenTokenAndReturnsNoContent()
    {
        var result = await CreateController().WithUser(Guid.NewGuid()).Logout(new LogoutRequest { RefreshToken = "refresh-raw" }, Ct);

        Assert.IsType<NoContentResult>(result);
        await _service.Received(1).LogoutAsync("refresh-raw", Ip, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ForgotPassword_ReturnsTheSameNeutralMessageWhateverTheEmail()
    {
        var request = new ForgotPasswordRequest { Email = "anyone@example.com" };

        var result = await CreateController().ForgotPassword(request, Ct);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("If that email is registered, a password reset link has been sent.", ok.JsonProperty("message"));
        await _service.Received(1).ForgotPasswordAsync(request, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResetPassword_CallsTheServiceAndReturnsASuccessMessage()
    {
        var request = new ResetPasswordRequest { Email = "artisan@example.com", Token = "t" };

        var result = await CreateController().ResetPassword(request, Ct);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Password has been reset successfully.", ok.JsonProperty("message"));
        await _service.Received(1).ResetPasswordAsync(request, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SwitchRole_UsesTheSignedInUserFromTheSubClaim()
    {
        var userId = Guid.NewGuid();
        _service.SwitchRoleAsync(userId, RoleNames.Producer, Arg.Any<CancellationToken>()).Returns(_response);

        var result = await CreateController().WithUser(userId).SwitchRole(new SwitchRoleRequest { Role = RoleNames.Producer }, Ct);

        Assert.Same(_response, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task SwitchRole_FallsBackToTheNameIdentifierClaim()
    {
        var userId = Guid.NewGuid();

        await CreateController().WithNameIdentifierUser(userId).SwitchRole(new SwitchRoleRequest { Role = RoleNames.Customer }, Ct);

        await _service.Received(1).SwitchRoleAsync(userId, RoleNames.Customer, Arg.Any<CancellationToken>());
    }
}
