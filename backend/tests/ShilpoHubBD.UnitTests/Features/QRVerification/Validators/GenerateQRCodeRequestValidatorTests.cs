using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.QRVerification;
using ShilpoHubBD.Application.Validators.QRVerification;

namespace ShilpoHubBD.UnitTests.Features.QRVerification.Validators;

[Trait("Feature", "QRVerification")]
[Trait("Layer", "Validator")]
public class GenerateQRCodeRequestValidatorTests
{
    private readonly GenerateQRCodeRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidProductId_HasNoErrors()
        => _validator.TestValidate(new GenerateQRCodeRequest { ProductId = Guid.NewGuid() }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_EmptyProductId_FailsOnProductId()
        => _validator.TestValidate(new GenerateQRCodeRequest { ProductId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.ProductId);
}
