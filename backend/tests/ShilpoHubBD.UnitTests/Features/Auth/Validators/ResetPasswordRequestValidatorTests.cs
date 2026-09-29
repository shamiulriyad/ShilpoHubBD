using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Auth;
using ShilpoHubBD.Application.Validators.Auth;

namespace ShilpoHubBD.UnitTests.Features.Auth.Validators;

[Trait("Feature", "Auth")]
[Trait("Layer", "Validator")]
public class ResetPasswordRequestValidatorTests
{
    private readonly ResetPasswordRequestValidator _validator = new();

    private static ResetPasswordRequest Valid() => new()
    {
        Email = "artisan@example.com",
        Token = "reset-token",
        NewPassword = "Heritage1",
        ConfirmPassword = "Heritage1",
    };

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
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
    public void Validate_TokenMissing_FailsOnToken(string token)
    {
        var request = Valid();
        request.Token = token;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Validate_NewPasswordExactlyEightCharacters_Passes()
        => _validator.TestValidate(WithNewPassword("Abcdefg1")).ShouldNotHaveValidationErrorFor(x => x.NewPassword);

    [Fact]
    public void Validate_NewPasswordSevenCharacters_FailsOnLength()
        => _validator.TestValidate(WithNewPassword("Abcdef1")).ShouldHaveValidationErrorFor(x => x.NewPassword);

    [Theory]
    [InlineData("abcdefg1", "Password must contain at least one uppercase letter.")]
    [InlineData("ABCDEFG1", "Password must contain at least one lowercase letter.")]
    [InlineData("Abcdefgh", "Password must contain at least one digit.")]
    public void Validate_NewPasswordMissingCharacterClass_FailsWithSpecificMessage(string password, string message)
        => _validator.TestValidate(WithNewPassword(password))
            .ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage(message);

    [Fact]
    public void Validate_EmptyNewPassword_FailsOnNewPassword()
        => _validator.TestValidate(WithNewPassword(string.Empty)).ShouldHaveValidationErrorFor(x => x.NewPassword);

    [Fact]
    public void Validate_ConfirmPasswordDiffers_FailsOnConfirmPassword()
    {
        var request = Valid();
        request.ConfirmPassword = "Heritage2";

        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("ConfirmPassword must match NewPassword.");
    }

    private static ResetPasswordRequest WithNewPassword(string password)
    {
        var request = Valid();
        request.NewPassword = password;
        request.ConfirmPassword = password;
        return request;
    }
}
