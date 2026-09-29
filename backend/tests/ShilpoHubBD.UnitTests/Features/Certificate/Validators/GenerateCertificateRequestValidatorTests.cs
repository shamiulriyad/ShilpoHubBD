using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Certificate;
using ShilpoHubBD.Application.Validators.Certificate;

namespace ShilpoHubBD.UnitTests.Features.Certificate.Validators;

[Trait("Feature", "Certificate")]
[Trait("Layer", "Validator")]
public class GenerateCertificateRequestValidatorTests
{
    private readonly GenerateCertificateRequestValidator _validator = new();

    [Fact]
    public void Validate_KnownProductId_HasNoErrors()
        => _validator.TestValidate(new GenerateCertificateRequest { ProductId = Guid.NewGuid() }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_EmptyProductId_FailsOnProductId()
        => _validator.TestValidate(new GenerateCertificateRequest { ProductId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.ProductId);
}
