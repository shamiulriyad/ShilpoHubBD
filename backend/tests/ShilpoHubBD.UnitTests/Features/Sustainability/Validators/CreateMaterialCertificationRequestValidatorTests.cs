using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Sustainability;
using ShilpoHubBD.Application.Validators.Sustainability;

namespace ShilpoHubBD.UnitTests.Features.Sustainability.Validators;

[Trait("Feature", "Sustainability")]
[Trait("Layer", "Validator")]
public class CreateMaterialCertificationRequestValidatorTests
{
    private readonly CreateMaterialCertificationRequestValidator _validator = new();

    private static CreateMaterialCertificationRequest Valid() => new()
    {
        MaterialName = "Cotton", CertifyingBody = "GOTS", CertificateReference = "GOTS-123", IssuedAt = DateTime.UtcNow,
    };

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_EmptyMaterialName_FailsOnMaterialName()
    {
        var request = Valid();
        request.MaterialName = "";
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.MaterialName);
    }

    [Fact]
    public void Validate_EmptyCertifyingBody_FailsOnCertifyingBody()
    {
        var request = Valid();
        request.CertifyingBody = "";
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.CertifyingBody);
    }

    [Fact]
    public void Validate_EmptyCertificateReference_FailsOnCertificateReference()
    {
        var request = Valid();
        request.CertificateReference = "";
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.CertificateReference);
    }

    [Fact]
    public void Validate_EmptyIssuedAt_FailsOnIssuedAt()
    {
        var request = Valid();
        request.IssuedAt = default;
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.IssuedAt);
    }

    [Fact]
    public void Validate_NullExpiresAt_HasNoError()
    {
        var request = Valid();
        request.ExpiresAt = null;
        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.ExpiresAt);
    }

    [Fact]
    public void Validate_ExpiresAtBeforeIssuedAt_FailsOnExpiresAt()
    {
        var request = Valid();
        request.IssuedAt = DateTime.UtcNow;
        request.ExpiresAt = DateTime.UtcNow.AddDays(-1);
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.ExpiresAt);
    }

    [Fact]
    public void Validate_ExpiresAtAfterIssuedAt_HasNoError()
    {
        var request = Valid();
        request.IssuedAt = DateTime.UtcNow;
        request.ExpiresAt = DateTime.UtcNow.AddYears(1);
        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.ExpiresAt);
    }
}
