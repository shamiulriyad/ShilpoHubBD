using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Validators.Reviews;

namespace ShilpoHubBD.UnitTests.Features.Reviews.Validators;

[Trait("Feature", "Reviews")]
[Trait("Layer", "Validator")]
public class ReviewQueryParametersValidatorTests
{
    private readonly ReviewQueryParametersValidator _validator = new();

    [Fact]
    public void Validate_Defaults_HaveNoErrors()
        => _validator.TestValidate(new ReviewQueryParameters()).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PageBelowOne_FailsOnPage(int page)
        => _validator.TestValidate(new ReviewQueryParameters { Page = page }).ShouldHaveValidationErrorFor(x => x.Page);

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Validate_PageSizeOutsideOneToFifty_FailsOnPageSize(int pageSize)
        => _validator.TestValidate(new ReviewQueryParameters { PageSize = pageSize }).ShouldHaveValidationErrorFor(x => x.PageSize);

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    public void Validate_PageSizeAtTheBounds_Passes(int pageSize)
        => _validator.TestValidate(new ReviewQueryParameters { PageSize = pageSize }).ShouldNotHaveValidationErrorFor(x => x.PageSize);
}
