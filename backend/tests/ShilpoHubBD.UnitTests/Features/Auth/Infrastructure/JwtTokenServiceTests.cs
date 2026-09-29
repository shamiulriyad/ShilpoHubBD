using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Infrastructure.Options;
using ShilpoHubBD.Infrastructure.Security;
using ShilpoHubBD.UnitTests.Common;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace ShilpoHubBD.UnitTests.Features.Auth.Infrastructure;

[Trait("Feature", "Auth")]
[Trait("Layer", "Infrastructure")]
public class JwtTokenServiceTests
{
    private const string SigningKey = "unit-test-signing-key-that-is-long-enough-for-hmac-sha256!!";

    private static readonly JwtSettings Settings = new()
    {
        Issuer = "shilpohub-tests",
        Audience = "shilpohub-app",
        Key = SigningKey,
        AccessTokenMinutes = 15,
        RefreshTokenDays = 7,
    };

    private static JwtTokenService CreateService(JwtSettings? settings = null) => new(MsOptions.Create(settings ?? Settings));

    private static ClaimsPrincipal Validate(string token, string key = SigningKey)
        => new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
        {
            ValidIssuer = Settings.Issuer,
            ValidAudience = Settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        }, out _);

    [Fact]
    public void GenerateAccessToken_ProducesATokenThatValidatesWithTheConfiguredKeyIssuerAndAudience()
    {
        var user = TestUsers.Create("artisan@example.com", "Rahima Begum");

        var token = CreateService().GenerateAccessToken(user, new[] { RoleNames.Producer }, null);

        var principal = Validate(token.AccessToken);
        Assert.Equal(user.Id.ToString(), principal.FindFirstValue(ClaimTypes.NameIdentifier));
    }

    [Fact]
    public void GenerateAccessToken_CarriesUserIdentityClaims()
    {
        var user = TestUsers.Create("artisan@example.com", "Rahima Begum");

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(
            CreateService().GenerateAccessToken(user, new[] { RoleNames.Customer }, null).AccessToken);

        Assert.Equal(user.Id.ToString(), jwt.Subject);
        Assert.Equal("artisan@example.com", jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("Rahima Begum", jwt.Claims.Single(c => c.Type == "name").Value);
        Assert.True(Guid.TryParse(jwt.Id, out _));
        Assert.Equal(Settings.Issuer, jwt.Issuer);
        Assert.Equal(Settings.Audience, Assert.Single(jwt.Audiences));
    }

    [Fact]
    public void GenerateAccessToken_AddsOneRoleClaimPerRole()
    {
        var token = CreateService().GenerateAccessToken(
            TestUsers.Create(), new[] { RoleNames.Customer, RoleNames.Producer }, null);

        var principal = Validate(token.AccessToken);
        Assert.True(principal.IsInRole(RoleNames.Customer));
        Assert.True(principal.IsInRole(RoleNames.Producer));
        Assert.False(principal.IsInRole(RoleNames.SuperAdmin));
    }

    [Fact]
    public void GenerateAccessToken_WithActiveRole_AddsActiveRoleClaim()
    {
        var token = CreateService().GenerateAccessToken(TestUsers.Create(), new[] { RoleNames.Producer }, RoleNames.Producer);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);
        Assert.Equal(RoleNames.Producer, jwt.Claims.Single(c => c.Type == "active_role").Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateAccessToken_WithoutActiveRole_OmitsActiveRoleClaim(string? activeRole)
    {
        var token = CreateService().GenerateAccessToken(TestUsers.Create(), new[] { RoleNames.Producer }, activeRole);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);
        Assert.DoesNotContain(jwt.Claims, c => c.Type == "active_role");
    }

    [Fact]
    public void GenerateAccessToken_ExpiresAfterTheConfiguredMinutesAndReportsThatExpiry()
    {
        var before = DateTime.UtcNow;

        var token = CreateService().GenerateAccessToken(TestUsers.Create(), Array.Empty<string>(), null);

        Assert.InRange(token.ExpiresAtUtc, before.AddMinutes(15), DateTime.UtcNow.AddMinutes(15));
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);
        Assert.Equal(token.ExpiresAtUtc, jwt.ValidTo, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GenerateAccessToken_EachTokenGetsAUniqueId()
    {
        var service = CreateService();
        var user = TestUsers.Create();
        var handler = new JwtSecurityTokenHandler();

        var first = handler.ReadJwtToken(service.GenerateAccessToken(user, Array.Empty<string>(), null).AccessToken).Id;
        var second = handler.ReadJwtToken(service.GenerateAccessToken(user, Array.Empty<string>(), null).AccessToken).Id;

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void GenerateAccessToken_TokenIsRejectedWhenCheckedWithAnotherKey()
    {
        var token = CreateService().GenerateAccessToken(TestUsers.Create(), new[] { RoleNames.Customer }, null);

        Assert.ThrowsAny<SecurityTokenException>(
            () => Validate(token.AccessToken, "a-completely-different-signing-key-of-sufficient-length!!"));
    }

    [Fact]
    public void GenerateAccessToken_SigningKeyTooShortForHmacSha256_IsRefused()
    {
        var weak = new JwtSettings { Issuer = Settings.Issuer, Audience = Settings.Audience, Key = "short" };

        Assert.ThrowsAny<ArgumentException>(
            () => CreateService(weak).GenerateAccessToken(TestUsers.Create(), Array.Empty<string>(), null));
    }

    [Fact]
    public void RefreshTokenLifetime_ComesFromTheConfiguredDays()
        => Assert.Equal(TimeSpan.FromDays(7), CreateService().RefreshTokenLifetime);

    [Fact]
    public void GenerateRefreshTokenValue_IsBase64Of64RandomBytes()
    {
        var value = CreateService().GenerateRefreshTokenValue();

        Assert.Equal(64, Convert.FromBase64String(value).Length);
    }

    [Fact]
    public void GenerateRefreshTokenValue_IsDifferentEveryTime()
    {
        var service = CreateService();

        Assert.NotEqual(service.GenerateRefreshTokenValue(), service.GenerateRefreshTokenValue());
    }

    [Fact]
    public void HashToken_IsUppercaseHexSha256OfTheUtf8Value()
        => Assert.Equal("BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD", CreateService().HashToken("abc"));

    [Fact]
    public void HashToken_IsStableForTheSameInputAndDiffersForAnother()
    {
        var service = CreateService();

        Assert.Equal(service.HashToken("raw-token"), service.HashToken("raw-token"));
        Assert.NotEqual(service.HashToken("raw-token"), service.HashToken("raw-token2"));
    }
}
