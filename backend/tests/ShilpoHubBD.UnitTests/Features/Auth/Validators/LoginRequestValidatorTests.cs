using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Auth;
using ShilpoHubBD.Application.Validators.Auth;

namespace ShilpoHubBD.UnitTests.Features.Auth.Validators;

[Trait("Feature", "Auth")]
[Trait("Layer", "Validator")]
public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    private static LoginRequest Valid() => new() { Email = "artisan@example.com", Password = "any-password" };

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    public void Validate_EmailMissingOrMalformed_FailsOnEmail(string email)
    {
        var request = Valid();
        request.Email = email;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_PasswordMissing_FailsOnPassword(string password)
    {
        var request = Valid();
        request.Password = password;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_ShortPassword_IsAllowedBecauseLoginDoesNotEnforceStrength()
    {
        var request = Valid();
        request.Password = "x";

        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}
