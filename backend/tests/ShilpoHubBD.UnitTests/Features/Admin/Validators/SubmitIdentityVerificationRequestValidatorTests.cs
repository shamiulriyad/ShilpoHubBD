using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.Validators.Admin;

namespace ShilpoHubBD.UnitTests.Features.Admin.Validators;

[Trait("Feature", "Admin")]
[Trait("Layer", "Validator")]
public class SubmitIdentityVerificationRequestValidatorTests
{
    private readonly SubmitIdentityVerificationRequestValidator _validator = new();

    private static SubmitIdentityVerificationRequest Valid() => new()
    {
        Type = "NationalId",
        DocumentNumber = "1234567890",
        FrontImageUrl = "https://cdn.example/front.jpg",
    };

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithOptionalFields_HasNoErrors()
    {
        var request = Valid();
        request.BackImageUrl = "https://cdn.example/back.jpg";
        request.SelfieImageUrl = "https://cdn.example/selfie.jpg";
        request.ApplicantNote = "Please expedite.";

        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Bogus")]
    [InlineData("National Id")]
    public void Validate_TypeMissingOrUnknown_FailsWithInvalidTypeMessage(string type)
    {
        var request = Valid();
        request.Type = type;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Type).WithErrorMessage("Invalid Type.");
    }

    public static TheoryData<string> KnownTypes() => new() { "NationalId", "Passport", "TradeLicense", "BusinessRegistration", "Other" };

    [Theory]
    [MemberData(nameof(KnownTypes))]
    public void Validate_EachKnownType_Passes(string type)
    {
        var request = Valid();
        request.Type = type;

        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void Validate_TypeInADifferentCase_IsAcceptedBecauseParsingIgnoresCase()
    {
        var request = Valid();
        request.Type = "nationalid";

        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_DocumentNumberMissing_FailsOnDocumentNumber(string number)
    {
        var request = Valid();
        request.DocumentNumber = number;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.DocumentNumber);
    }

    [Fact]
    public void Validate_DocumentNumberOverMaximumLength_FailsOnDocumentNumber()
    {
        var request = Valid();
        request.DocumentNumber = new string('1', 101);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.DocumentNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_FrontImageUrlMissing_FailsOnFrontImageUrl(string url)
    {
        var request = Valid();
        request.FrontImageUrl = url;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.FrontImageUrl);
    }

    [Fact]
    public void Validate_FrontImageUrlOverMaximumLength_FailsOnFrontImageUrl()
    {
        var request = Valid();
        request.FrontImageUrl = "https://cdn.example/" + new string('a', 2000);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.FrontImageUrl);
    }

    [Fact]
    public void Validate_BackImageUrlOverMaximumLength_FailsOnBackImageUrl()
    {
        var request = Valid();
        request.BackImageUrl = new string('a', 2001);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.BackImageUrl);
    }

    [Fact]
    public void Validate_SelfieImageUrlOverMaximumLength_FailsOnSelfieImageUrl()
    {
        var request = Valid();
        request.SelfieImageUrl = new string('a', 2001);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.SelfieImageUrl);
    }

    [Fact]
    public void Validate_ApplicantNoteOverMaximumLength_FailsOnApplicantNote()
    {
        var request = Valid();
        request.ApplicantNote = new string('n', 1001);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.ApplicantNote);
    }
}
