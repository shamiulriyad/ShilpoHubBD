using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Certificate;
using ShilpoHubBD.Application.Validators.Certificate;

namespace ShilpoHubBD.UnitTests.Features.Certificate.Validators;

[Trait("Feature", "Certificate")]
[Trait("Layer", "Validator")]
public class VerifyCertificateRequestValidatorTests
{
    private readonly VerifyCertificateRequestValidator _validator = new();

    [Fact]
    public void Validate_CertificateNumberGiven_HasNoErrors()
        => _validator.TestValidate(new VerifyCertificateRequest { CertificateNumber = "SH-20260101-ABCD1234" })
            .ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_CertificateNumberMissing_FailsOnCertificateNumber(string number)
        => _validator.TestValidate(new VerifyCertificateRequest { CertificateNumber = number })
            .ShouldHaveValidationErrorFor(x => x.CertificateNumber);

    [Fact]
    public void Validate_CertificateNumberAtMaximumLength_Passes()
        => _validator.TestValidate(new VerifyCertificateRequest { CertificateNumber = new string('a', 50) })
            .ShouldNotHaveValidationErrorFor(x => x.CertificateNumber);

    [Fact]
    public void Validate_CertificateNumberOverMaximumLength_FailsOnCertificateNumber()
        => _validator.TestValidate(new VerifyCertificateRequest { CertificateNumber = new string('a', 51) })
            .ShouldHaveValidationErrorFor(x => x.CertificateNumber);
}
