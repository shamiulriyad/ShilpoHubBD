using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.QRVerification;
using ShilpoHubBD.Application.Validators.QRVerification;

namespace ShilpoHubBD.UnitTests.Features.QRVerification.Validators;

[Trait("Feature", "QRVerification")]
[Trait("Layer", "Validator")]
public class QRVerificationQueryParametersValidatorTests
{
    private readonly QRVerificationQueryParametersValidator _validator = new();

    [Fact]
    public void Validate_Defaults_HaveNoErrors()
        => _validator.TestValidate(new QRVerificationQueryParameters()).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PageBelowOne_FailsOnPage(int page)
        => _validator.TestValidate(new QRVerificationQueryParameters { Page = page }).ShouldHaveValidationErrorFor(x => x.Page);

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Validate_PageSizeOutsideOneToFifty_FailsOnPageSize(int pageSize)
        => _validator.TestValidate(new QRVerificationQueryParameters { PageSize = pageSize }).ShouldHaveValidationErrorFor(x => x.PageSize);

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    public void Validate_PageSizeAtTheBounds_Passes(int pageSize)
        => _validator.TestValidate(new QRVerificationQueryParameters { PageSize = pageSize }).ShouldNotHaveValidationErrorFor(x => x.PageSize);
}
