using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Auth;
using ShilpoHubBD.Application.Validators.Auth;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.UnitTests.Features.Auth.Validators;

[Trait("Feature", "Auth")]
[Trait("Layer", "Validator")]
public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    private static RegisterRequest Valid() => new()
    {
        Email = "new.artisan@example.com",
        Password = "Heritage1",
        ConfirmPassword = "Heritage1",
        FullName = "Rahima Begum",
        Roles = new List<string> { RoleNames.Customer },
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

    [Fact]
    public void Validate_PasswordExactlyEightCharacters_Passes()
    {
        var request = WithPassword("Abcdefg1");

        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_PasswordSevenCharacters_FailsOnLength()
    {
        var request = WithPassword("Abcdef1");

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("abcdefg1", "Password must contain at least one uppercase letter.")]
    [InlineData("ABCDEFG1", "Password must contain at least one lowercase letter.")]
    [InlineData("Abcdefgh", "Password must contain at least one digit.")]
    public void Validate_PasswordMissingCharacterClass_FailsWithSpecificMessage(string password, string message)
    {
        var request = WithPassword(password);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage(message);
    }

    [Fact]
    public void Validate_EmptyPassword_FailsOnPassword()
    {
        var request = WithPassword(string.Empty);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_ConfirmPasswordDiffers_FailsOnConfirmPassword()
    {
        var request = Valid();
        request.ConfirmPassword = "Heritage2";

        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("ConfirmPassword must match Password.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_FullNameMissing_FailsOnFullName(string fullName)
    {
        var request = Valid();
        request.FullName = fullName;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_FullNameAtMaximumLength_Passes()
    {
        var request = Valid();
        request.FullName = new string('a', 200);

        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_FullNameOverMaximumLength_FailsOnFullName()
    {
        var request = Valid();
        request.FullName = new string('a', 201);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_NoRoles_FailsWithRoleSelectionMessage()
    {
        var request = Valid();
        request.Roles = new List<string>();

        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Roles)
            .WithErrorMessage("At least one role must be selected.");
    }

    public static TheoryData<string> SelfRegisterableRoles() => new(RoleNames.SelfRegisterableRoles);

    [Theory]
    [MemberData(nameof(SelfRegisterableRoles))]
    public void Validate_EachSelfRegisterableRole_Passes(string role)
    {
        var request = Valid();
        request.Roles = new List<string> { role };

        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_SeveralSelfRegisterableRoles_Passes()
    {
        var request = Valid();
        request.Roles = new List<string> { RoleNames.Customer, RoleNames.Producer };

        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(RoleNames.SuperAdmin)]
    [InlineData("Hacker")]
    [InlineData("customer")]
    public void Validate_RoleThatCannotSelfRegister_FailsOnThatRole(string role)
    {
        var request = Valid();
        request.Roles = new List<string> { RoleNames.Customer, role };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor("Roles[1]");
    }

    private static RegisterRequest WithPassword(string password)
    {
        var request = Valid();
        request.Password = password;
        request.ConfirmPassword = password;
        return request;
    }
}
