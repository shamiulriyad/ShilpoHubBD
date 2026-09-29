using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.QRVerification;
using ShilpoHubBD.Application.Validators.QRVerification;

namespace ShilpoHubBD.UnitTests.Features.QRVerification.Validators;

[Trait("Feature", "QRVerification")]
[Trait("Layer", "Validator")]
public class VerifyQRRequestValidatorTests
{
    private readonly VerifyQRRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidCode_HasNoErrors()
        => _validator.TestValidate(new VerifyQRRequest { Code = "abc123" }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_EmptyCode_FailsOnCode()
        => _validator.TestValidate(new VerifyQRRequest { Code = "" }).ShouldHaveValidationErrorFor(x => x.Code);

    [Fact]
    public void Validate_CodeTooLong_FailsOnCode()
        => _validator.TestValidate(new VerifyQRRequest { Code = new string('a', 101) }).ShouldHaveValidationErrorFor(x => x.Code);

    [Fact]
    public void Validate_CodeAtMaxLength_HasNoError()
        => _validator.TestValidate(new VerifyQRRequest { Code = new string('a', 100) }).ShouldNotHaveValidationErrorFor(x => x.Code);
}
