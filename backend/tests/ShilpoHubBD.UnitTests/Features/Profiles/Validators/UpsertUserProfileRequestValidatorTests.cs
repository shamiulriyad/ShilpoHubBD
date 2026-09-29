using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Profiles;
using ShilpoHubBD.Application.Validators.Profiles;

namespace ShilpoHubBD.UnitTests.Features.Profiles.Validators;

[Trait("Feature", "Profiles")]
[Trait("Layer", "Validator")]
public class UpsertUserProfileRequestValidatorTests
{
    private readonly UpsertUserProfileRequestValidator _validator = new();

    private static UpsertUserProfileRequest Valid() => new()
    {
        LegalName = "Rahima Begum",
        Phone = "+8801712345678",
        NidNumber = "1234567890",
        AddressLine = "House 12, Road 3, Dhaka",
    };

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithOptionalFields_HasNoErrors()
    {
        var request = Valid();
        request.Expertise = "Jamdani weaving";
        request.DistrictId = Guid.NewGuid();
        request.About = "Third-generation weaver.";

        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_LegalNameMissing_FailsOnLegalName(string name)
    {
        var request = Valid();
        request.LegalName = name;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.LegalName);
    }

    [Fact]
    public void Validate_LegalNameOverMaximumLength_FailsOnLegalName()
    {
        var request = Valid();
        request.LegalName = new string('a', 201);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.LegalName);
    }

    [Theory]
    [InlineData("01712345678")]
    [InlineData("+880 1712-345678")]
    [InlineData("0171234")]
    public void Validate_PhoneInAnAcceptedShape_Passes(string phone)
    {
        var request = Valid();
        request.Phone = phone;

        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc123")]
    [InlineData("017123")]
    public void Validate_PhoneMissingOrTooShortOrNotNumeric_FailsWithAValidPhoneMessage(string phone)
    {
        var request = Valid();
        request.Phone = phone;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Phone).WithErrorMessage("Enter a valid phone number.");
    }

    [Theory]
    [InlineData("1234567890")]
    [InlineData("1234567890123")]
    [InlineData("12345678901234567")]
    public void Validate_NidWithAnAcceptedDigitCount_Passes(string nid)
    {
        var request = Valid();
        request.NidNumber = nid;

        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.NidNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("12345abcde")]
    public void Validate_NidWithTheWrongDigitCountOrNonDigits_FailsWithTheDigitCountMessage(string nid)
    {
        var request = Valid();
        request.NidNumber = nid;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.NidNumber)
            .WithErrorMessage("The NID number must be 10, 13 or 17 digits.");
    }

    [Fact]
    public void Validate_ExpertiseOverMaximumLength_FailsOnExpertise()
    {
        var request = Valid();
        request.Expertise = new string('e', 201);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Expertise);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_AddressLineMissing_FailsOnAddressLine(string address)
    {
        var request = Valid();
        request.AddressLine = address;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.AddressLine);
    }

    [Fact]
    public void Validate_AddressLineOverMaximumLength_FailsOnAddressLine()
    {
        var request = Valid();
        request.AddressLine = new string('a', 501);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.AddressLine);
    }

    [Fact]
    public void Validate_AboutOverMaximumLength_FailsOnAbout()
    {
        var request = Valid();
        request.About = new string('a', 2001);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.About);
    }
}
