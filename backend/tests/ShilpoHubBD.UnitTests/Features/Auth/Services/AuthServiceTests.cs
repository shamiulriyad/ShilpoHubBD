using Microsoft.Extensions.Configuration;
using ShilpoHubBD.Application.DTOs.Auth;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Services.Auth;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Security;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Auth.Services;

[Trait("Feature", "Auth")]
[Trait("Layer", "Service")]
public class AuthServiceTests
{
    private const string Ip = "203.0.113.7";
    private static readonly TimeSpan RefreshLifetime = TimeSpan.FromDays(7);
    private static readonly DateTime AccessExpiry = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IRoleRepository _roles = Substitute.For<IRoleRepository>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IPasswordResetTokenRepository _resetTokens = Substitute.For<IPasswordResetTokenRepository>();
    private readonly ITokenService _tokens = Substitute.For<ITokenService>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IEmailSender _email = Substitute.For<IEmailSender>();
    private readonly IThreatDetectionRepository _threats = Substitute.For<IThreatDetectionRepository>();
    private readonly List<RefreshToken> _addedRefreshTokens = new();

    public AuthServiceTests()
    {
        _tokens.GenerateAccessToken(Arg.Any<User>(), Arg.Any<IEnumerable<string>>(), Arg.Any<string?>())
            .Returns(new GeneratedAccessToken("access-token", AccessExpiry));
        _tokens.GenerateRefreshTokenValue().Returns("raw-refresh-token");
        _tokens.HashToken(Arg.Any<string>()).Returns(call => "hash:" + call.Arg<string>());
        _tokens.RefreshTokenLifetime.Returns(RefreshLifetime);
        _hasher.Hash(Arg.Any<string>()).Returns(call => "bcrypt:" + call.Arg<string>());
        _refreshTokens.AddAsync(Arg.Do<RefreshToken>(_addedRefreshTokens.Add), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private AuthService CreateService(IConfiguration? configuration = null)
        => new(_users, _roles, _refreshTokens, _resetTokens, _tokens, _hasher, _email, configuration ?? TestConfiguration.Empty(), _threats);

    // ---------- RegisterAsync ----------

    private static RegisterRequest RegisterRequest(params string[] roles) => new()
    {
        Email = "  New.Artisan@Example.COM ",
        Password = "Heritage1",
        ConfirmPassword = "Heritage1",
        FullName = "  Rahima Begum  ",
        Roles = roles.ToList(),
    };

    [Fact]
    public async Task RegisterAsync_NewEmail_SavesActiveUserWithNormalizedEmailTrimmedNameAndHashedPassword()
    {
        var customer = TestUsers.Role(RoleNames.Customer);
        _roles.GetByNamesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>()).Returns(new List<Role> { customer });
        User? saved = null;
        await _users.AddAsync(Arg.Do<User>(u => saved = u), Arg.Any<CancellationToken>());
        var before = DateTime.UtcNow;

        await CreateService().RegisterAsync(RegisterRequest(RoleNames.Customer), Ip, Ct);

        Assert.NotNull(saved);
        Assert.Equal("new.artisan@example.com", saved.Email);
        Assert.Equal("Rahima Begum", saved.FullName);
        Assert.Equal("bcrypt:Heritage1", saved.PasswordHash);
        Assert.True(saved.IsActive);
        Assert.InRange(saved.CreatedAt, before, DateTime.UtcNow);
        Assert.Equal(saved.CreatedAt, saved.UpdatedAt);
        var userRole = Assert.Single(saved.UserRoles);
        Assert.Equal(customer.Id, userRole.RoleId);
        Assert.Equal(saved.Id, userRole.UserId);
        Assert.Null(userRole.AssignedByUserId);
        await _users.Received(1).ExistsByEmailAsync("new.artisan@example.com", Arg.Any<CancellationToken>());
        await _users.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_SingleRole_ReturnsTokensWithThatRoleActive()
    {
        _roles.GetByNamesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Role> { TestUsers.Role(RoleNames.Producer) });

        var response = await CreateService().RegisterAsync(RegisterRequest(RoleNames.Producer), Ip, Ct);

        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("raw-refresh-token", response.RefreshToken);
        Assert.Equal(AccessExpiry, response.ExpiresAtUtc);
        Assert.Equal("new.artisan@example.com", response.Email);
        Assert.Equal("Rahima Begum", response.FullName);
        Assert.Equal(new[] { RoleNames.Producer }, response.Roles);
        Assert.Equal(RoleNames.Producer, response.ActiveRole);
        _tokens.Received(1).GenerateAccessToken(
            Arg.Is<User>(u => u.Id == response.UserId),
            Arg.Is<IEnumerable<string>>(r => r.SequenceEqual(new[] { RoleNames.Producer })),
            RoleNames.Producer);
    }

    [Fact]
    public async Task RegisterAsync_SeveralRoles_LeavesActiveRoleUnset()
    {
        _roles.GetByNamesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Role> { TestUsers.Role(RoleNames.Customer), TestUsers.Role(RoleNames.Tourist) });

        var response = await CreateService().RegisterAsync(RegisterRequest(RoleNames.Customer, RoleNames.Tourist), Ip, Ct);

        Assert.Null(response.ActiveRole);
        Assert.Equal(new[] { RoleNames.Customer, RoleNames.Tourist }, response.Roles);
    }

    [Fact]
    public async Task RegisterAsync_StoresHashedRefreshTokenForTheNewUserAndIp()
    {
        _roles.GetByNamesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Role> { TestUsers.Role(RoleNames.Customer) });
        var before = DateTime.UtcNow;

        var response = await CreateService().RegisterAsync(RegisterRequest(RoleNames.Customer), Ip, Ct);

        var stored = Assert.Single(_addedRefreshTokens);
        Assert.Equal(response.UserId, stored.UserId);
        Assert.Equal("hash:raw-refresh-token", stored.TokenHash);
        Assert.Equal(Ip, stored.CreatedByIp);
        Assert.InRange(stored.ExpiresAt, before.Add(RefreshLifetime), DateTime.UtcNow.Add(RefreshLifetime));
        await _refreshTokens.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_DuplicateRoleNamesInDifferentCase_AreLookedUpOnce()
    {
        _roles.GetByNamesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Role> { TestUsers.Role(RoleNames.Customer) });

        await CreateService().RegisterAsync(RegisterRequest(RoleNames.Customer, "customer"), Ip, Ct);

        await _roles.Received(1).GetByNamesAsync(
            Arg.Is<IEnumerable<string>>(names => names.Count() == 1), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_EmailAlreadyRegistered_ThrowsConflictAndSavesNothing()
    {
        _users.ExistsByEmailAsync("new.artisan@example.com", Arg.Any<CancellationToken>()).Returns(true);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().RegisterAsync(RegisterRequest(RoleNames.Customer), Ip, Ct));

        Assert.Equal("Email is already registered.", error.Message);
        await _users.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _users.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_RequestedRoleDoesNotExist_ThrowsNotFoundAndSavesNothing()
    {
        _roles.GetByNamesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Role> { TestUsers.Role(RoleNames.Customer) });

        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().RegisterAsync(RegisterRequest(RoleNames.Customer, RoleNames.Producer), Ip, Ct));

        Assert.Equal("One or more requested roles do not exist.", error.Message);
        await _users.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        Assert.Empty(_addedRefreshTokens);
    }

    // ---------- LoginAsync ----------

    private User ActiveUserWithPassword(params string[] roles)
    {
        var user = TestUsers.Create("artisan@example.com", passwordHash: "stored-hash").WithRoles(roles);
        _users.GetByEmailWithRolesAsync("artisan@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _hasher.Verify("Correct1", "stored-hash").Returns(true);
        return user;
    }

    private static LoginRequest Login(string password = "Correct1") => new() { Email = " Artisan@Example.com ", Password = password };

    private List<LoginAttempt> CaptureAttempts()
    {
        var attempts = new List<LoginAttempt>();
        _threats.AddLoginAttemptAsync(Arg.Do<LoginAttempt>(attempts.Add), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        return attempts;
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_RecordsSuccessfulAttemptAndIssuesTokens()
    {
        var user = ActiveUserWithPassword(RoleNames.Producer);
        var attempts = CaptureAttempts();

        var response = await CreateService().LoginAsync(Login(), Ip, Ct);

        var attempt = Assert.Single(attempts);
        Assert.True(attempt.Succeeded);
        Assert.Equal("artisan@example.com", attempt.Email);
        Assert.Equal(Ip, attempt.IpAddress);
        Assert.Equal(user.Id, attempt.UserId);
        await _threats.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal(user.Id, response.UserId);
        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal(RoleNames.Producer, response.ActiveRole);
        Assert.Equal(Ip, Assert.Single(_addedRefreshTokens).CreatedByIp);
    }

    [Fact]
    public async Task LoginAsync_UserWithSeveralRoles_LeavesActiveRoleUnset()
    {
        ActiveUserWithPassword(RoleNames.Customer, RoleNames.Producer);

        var response = await CreateService().LoginAsync(Login(), Ip, Ct);

        Assert.Null(response.ActiveRole);
        Assert.Equal(new[] { RoleNames.Customer, RoleNames.Producer }, response.Roles);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_RecordsFailedAttemptAndThrowsWithoutTokens()
    {
        var user = ActiveUserWithPassword(RoleNames.Customer);
        var attempts = CaptureAttempts();

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateService().LoginAsync(Login("Wrong1"), Ip, Ct));

        Assert.Equal("Invalid email or password.", error.Message);
        var attempt = Assert.Single(attempts);
        Assert.False(attempt.Succeeded);
        Assert.Equal(user.Id, attempt.UserId);
        Assert.Empty(_addedRefreshTokens);
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_RecordsFailedAttemptAndGivesTheSameMessageAsWrongPassword()
    {
        var attempts = CaptureAttempts();

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateService().LoginAsync(Login(), Ip, Ct));

        Assert.Equal("Invalid email or password.", error.Message);
        var attempt = Assert.Single(attempts);
        Assert.False(attempt.Succeeded);
        Assert.Null(attempt.UserId);
        _hasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task LoginAsync_DisabledAccountWithCorrectPassword_RecordsFailedAttemptAndThrowsDisabled()
    {
        var user = ActiveUserWithPassword(RoleNames.Customer);
        user.IsActive = false;
        var attempts = CaptureAttempts();

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateService().LoginAsync(Login(), Ip, Ct));

        Assert.Equal("This account has been disabled.", error.Message);
        Assert.False(Assert.Single(attempts).Succeeded);
        Assert.Empty(_addedRefreshTokens);
    }

    [Fact]
    public async Task LoginAsync_BlockedIp_ThrowsBeforeCheckingCredentialsOrRecordingAnAttempt()
    {
        _threats.IsIpBlockedAsync(Ip, Arg.Any<CancellationToken>()).Returns(true);

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateService().LoginAsync(Login(), Ip, Ct));

        Assert.Equal("Access from this IP address has been blocked.", error.Message);
        await _users.DidNotReceive().GetByEmailWithRolesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _threats.DidNotReceive().AddLoginAttemptAsync(Arg.Any<LoginAttempt>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task LoginAsync_NoClientIp_SkipsTheBlockedIpCheck(string? ip)
    {
        ActiveUserWithPassword(RoleNames.Customer);

        await CreateService().LoginAsync(Login(), ip, Ct);

        await _threats.DidNotReceive().IsIpBlockedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ---------- RefreshTokenAsync ----------

    private RefreshToken StoredToken(Guid userId, DateTime? revokedAt = null, DateTime? expiresAt = null)
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = "hash:old-raw",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(6),
            RevokedAt = revokedAt,
        };
        _refreshTokens.GetByTokenHashAsync("hash:old-raw", Arg.Any<CancellationToken>()).Returns(token);
        return token;
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_RotatesItAndReturnsNewTokens()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Tourist);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        var old = StoredToken(user.Id);
        _tokens.GenerateRefreshTokenValue().Returns("new-raw");

        var response = await CreateService().RefreshTokenAsync("old-raw", Ip, Ct);

        Assert.NotNull(old.RevokedAt);
        Assert.Equal(Ip, old.RevokedByIp);
        Assert.Equal("hash:new-raw", old.ReplacedByTokenHash);
        Assert.Equal("Rotated on refresh.", old.ReasonRevoked);
        var replacement = Assert.Single(_addedRefreshTokens);
        Assert.Equal("hash:new-raw", replacement.TokenHash);
        Assert.Equal(user.Id, replacement.UserId);
        Assert.Equal(Ip, replacement.CreatedByIp);
        Assert.Equal("new-raw", response.RefreshToken);
        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal(RoleNames.Tourist, response.ActiveRole);
        await _refreshTokens.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshTokenAsync_UnknownToken_ThrowsInvalidRefreshToken()
    {
        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateService().RefreshTokenAsync("old-raw", Ip, Ct));

        Assert.Equal("Invalid refresh token.", error.Message);
        Assert.Empty(_addedRefreshTokens);
    }

    [Fact]
    public async Task RefreshTokenAsync_ReusedRevokedToken_RevokesEveryActiveTokenOfTheUserAndThrows()
    {
        var userId = Guid.NewGuid();
        StoredToken(userId, revokedAt: DateTime.UtcNow.AddHours(-1));

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateService().RefreshTokenAsync("old-raw", Ip, Ct));

        Assert.Equal("Invalid refresh token.", error.Message);
        await _refreshTokens.Received(1).RevokeAllActiveForUserAsync(
            userId, Ip, "Reuse of a revoked refresh token was detected.", Arg.Any<CancellationToken>());
        Assert.Empty(_addedRefreshTokens);
    }

    [Fact]
    public async Task RefreshTokenAsync_ExpiredToken_ThrowsExpiredWithoutRevokingOtherTokens()
    {
        StoredToken(Guid.NewGuid(), expiresAt: DateTime.UtcNow.AddMinutes(-1));

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateService().RefreshTokenAsync("old-raw", Ip, Ct));

        Assert.Equal("Refresh token has expired.", error.Message);
        await _refreshTokens.DidNotReceive().RevokeAllActiveForUserAsync(
            Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshTokenAsync_UserNoLongerExists_ThrowsInvalidRefreshToken()
    {
        StoredToken(Guid.NewGuid());

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateService().RefreshTokenAsync("old-raw", Ip, Ct));

        Assert.Equal("Invalid refresh token.", error.Message);
    }

    [Fact]
    public async Task RefreshTokenAsync_UserDisabled_ThrowsAndKeepsTheOldTokenUnrotated()
    {
        var user = TestUsers.Create(isActive: false).WithRoles(RoleNames.Customer);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        var old = StoredToken(user.Id);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateService().RefreshTokenAsync("old-raw", Ip, Ct));

        Assert.Null(old.RevokedAt);
        Assert.Empty(_addedRefreshTokens);
    }

    // ---------- LogoutAsync ----------

    [Fact]
    public async Task LogoutAsync_ActiveToken_RevokesItWithLogoutReason()
    {
        var token = StoredToken(Guid.NewGuid());

        await CreateService().LogoutAsync("old-raw", Ip, Ct);

        Assert.NotNull(token.RevokedAt);
        Assert.Equal(Ip, token.RevokedByIp);
        Assert.Equal("User logged out.", token.ReasonRevoked);
        await _refreshTokens.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogoutAsync_UnknownToken_DoesNothing()
    {
        await CreateService().LogoutAsync("old-raw", Ip, Ct);

        await _refreshTokens.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogoutAsync_AlreadyRevokedToken_KeepsTheOriginalRevocation()
    {
        var revokedAt = DateTime.UtcNow.AddHours(-2);
        var token = StoredToken(Guid.NewGuid(), revokedAt: revokedAt);
        token.ReasonRevoked = "Rotated on refresh.";

        await CreateService().LogoutAsync("old-raw", Ip, Ct);

        Assert.Equal(revokedAt, token.RevokedAt);
        Assert.Equal("Rotated on refresh.", token.ReasonRevoked);
        await _refreshTokens.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogoutAsync_ExpiredToken_DoesNothing()
    {
        var token = StoredToken(Guid.NewGuid(), expiresAt: DateTime.UtcNow.AddMinutes(-5));

        await CreateService().LogoutAsync("old-raw", Ip, Ct);

        Assert.Null(token.RevokedAt);
        await _refreshTokens.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ---------- ForgotPasswordAsync ----------

    [Fact]
    public async Task ForgotPasswordAsync_UnknownEmail_ReturnsQuietlyWithoutTokenOrEmail()
    {
        await CreateService().ForgotPasswordAsync(new ForgotPasswordRequest { Email = "nobody@example.com" }, Ct);

        await _resetTokens.DidNotReceive().AddAsync(Arg.Any<PasswordResetToken>(), Arg.Any<CancellationToken>());
        await _email.DidNotReceive().SendPasswordResetEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ForgotPasswordAsync_KnownEmail_StoresOneHourHashedTokenAndEmailsTheConfiguredLink()
    {
        var user = TestUsers.Create("artisan@example.com");
        _users.GetByEmailWithRolesAsync("artisan@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _tokens.GenerateRefreshTokenValue().Returns("a+b/c=");
        PasswordResetToken? stored = null;
        await _resetTokens.AddAsync(Arg.Do<PasswordResetToken>(t => stored = t), Arg.Any<CancellationToken>());
        var configuration = TestConfiguration.From(("Frontend:ResetPasswordUrl", "https://shilpohub.example/reset"));
        var before = DateTime.UtcNow;

        await CreateService(configuration).ForgotPasswordAsync(new ForgotPasswordRequest { Email = " Artisan@Example.com " }, Ct);

        Assert.NotNull(stored);
        Assert.Equal(user.Id, stored.UserId);
        Assert.Equal("hash:a+b/c=", stored.TokenHash);
        Assert.InRange(stored.ExpiresAt, before.AddHours(1), DateTime.UtcNow.AddHours(1));
        await _resetTokens.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _email.Received(1).SendPasswordResetEmailAsync(
            "artisan@example.com",
            "https://shilpohub.example/reset?email=artisan%40example.com&token=a%2Bb%2Fc%3D",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ForgotPasswordAsync_NoConfiguredUrl_UsesTheLocalFrontendDefault()
    {
        _users.GetByEmailWithRolesAsync("artisan@example.com", Arg.Any<CancellationToken>()).Returns(TestUsers.Create("artisan@example.com"));

        await CreateService().ForgotPasswordAsync(new ForgotPasswordRequest { Email = "artisan@example.com" }, Ct);

        await _email.Received(1).SendPasswordResetEmailAsync(
            Arg.Any<string>(),
            Arg.Is<string>(link => link.StartsWith("http://localhost:5173/reset-password?email=", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    // ---------- ResetPasswordAsync ----------

    private static ResetPasswordRequest Reset() => new()
    {
        Email = " Artisan@Example.com ",
        Token = "reset-raw",
        NewPassword = "NewHeritage1",
        ConfirmPassword = "NewHeritage1",
    };

    [Fact]
    public async Task ResetPasswordAsync_ValidToken_ChangesPasswordMarksTokenUsedAndRevokesSessions()
    {
        var user = TestUsers.Create("artisan@example.com", passwordHash: "old-hash");
        _users.GetByEmailWithRolesAsync("artisan@example.com", Arg.Any<CancellationToken>()).Returns(user);
        var resetToken = new PasswordResetToken { Id = Guid.NewGuid(), UserId = user.Id, ExpiresAt = DateTime.UtcNow.AddMinutes(30) };
        _resetTokens.GetActiveByTokenHashAsync("hash:reset-raw", user.Id, Arg.Any<CancellationToken>()).Returns(resetToken);
        var before = DateTime.UtcNow;

        await CreateService().ResetPasswordAsync(Reset(), Ct);

        Assert.Equal("bcrypt:NewHeritage1", user.PasswordHash);
        Assert.InRange(user.UpdatedAt, before, DateTime.UtcNow);
        Assert.NotNull(resetToken.UsedAt);
        await _refreshTokens.Received(1).RevokeAllActiveForUserAsync(user.Id, null, "Password was reset.", Arg.Any<CancellationToken>());
        await _users.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResetPasswordAsync_UnknownEmail_ThrowsInvalidOrExpired()
    {
        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateService().ResetPasswordAsync(Reset(), Ct));

        Assert.Equal("Invalid or expired reset token.", error.Message);
        await _resetTokens.DidNotReceive().GetActiveByTokenHashAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResetPasswordAsync_NoActiveToken_ThrowsAndLeavesThePasswordUnchanged()
    {
        var user = TestUsers.Create("artisan@example.com", passwordHash: "old-hash");
        _users.GetByEmailWithRolesAsync("artisan@example.com", Arg.Any<CancellationToken>()).Returns(user);

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateService().ResetPasswordAsync(Reset(), Ct));

        Assert.Equal("Invalid or expired reset token.", error.Message);
        Assert.Equal("old-hash", user.PasswordHash);
        await _users.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _refreshTokens.DidNotReceive().RevokeAllActiveForUserAsync(
            Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ---------- SwitchRoleAsync ----------

    [Fact]
    public async Task SwitchRoleAsync_HeldRole_IssuesTokensWithThatRoleActive()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Customer, RoleNames.Producer);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var response = await CreateService().SwitchRoleAsync(user.Id, RoleNames.Producer, Ct);

        Assert.Equal(RoleNames.Producer, response.ActiveRole);
        Assert.Equal(new[] { RoleNames.Customer, RoleNames.Producer }, response.Roles);
        Assert.Null(Assert.Single(_addedRefreshTokens).CreatedByIp);
    }

    [Fact]
    public async Task SwitchRoleAsync_RoleNameInDifferentCase_IsAccepted()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Producer);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var response = await CreateService().SwitchRoleAsync(user.Id, "producer", Ct);

        Assert.Equal("producer", response.ActiveRole);
    }

    [Fact]
    public async Task SwitchRoleAsync_RoleNotHeld_ThrowsUnauthorized()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Customer);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CreateService().SwitchRoleAsync(user.Id, RoleNames.SuperAdmin, Ct));

        Assert.Equal("You do not hold the requested role.", error.Message);
        Assert.Empty(_addedRefreshTokens);
    }

    [Fact]
    public async Task SwitchRoleAsync_UnknownUser_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().SwitchRoleAsync(Guid.NewGuid(), RoleNames.Customer, Ct));

        Assert.Equal("User not found.", error.Message);
    }
}
