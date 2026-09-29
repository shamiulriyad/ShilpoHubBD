using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Auth;
using ShilpoHubBD.Application.Validators.Auth;

namespace ShilpoHubBD.UnitTests.Features.Auth.Validators;

[Trait("Feature", "Auth")]
[Trait("Layer", "Validator")]
public class RefreshTokenRequestValidatorTests
{
    private readonly RefreshTokenRequestValidator _validator = new();

    [Fact]
    public void Validate_RefreshTokenPresent_HasNoErrors()
        => _validator.TestValidate(new RefreshTokenRequest { RefreshToken = "raw-refresh-token" }).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RefreshTokenMissing_FailsOnRefreshToken(string token)
        => _validator.TestValidate(new RefreshTokenRequest { RefreshToken = token }).ShouldHaveValidationErrorFor(x => x.RefreshToken);
}
