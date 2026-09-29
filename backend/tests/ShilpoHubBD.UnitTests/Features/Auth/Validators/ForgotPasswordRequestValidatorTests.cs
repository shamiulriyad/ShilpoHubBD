using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Auth;
using ShilpoHubBD.Application.Validators.Auth;

namespace ShilpoHubBD.UnitTests.Features.Auth.Validators;

[Trait("Feature", "Auth")]
[Trait("Layer", "Validator")]
public class ForgotPasswordRequestValidatorTests
{
    private readonly ForgotPasswordRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidEmail_HasNoErrors()
        => _validator.TestValidate(new ForgotPasswordRequest { Email = "artisan@example.com" }).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("artisan.example.com")]
    public void Validate_EmailMissingOrMalformed_FailsOnEmail(string email)
        => _validator.TestValidate(new ForgotPasswordRequest { Email = email }).ShouldHaveValidationErrorFor(x => x.Email);
}
