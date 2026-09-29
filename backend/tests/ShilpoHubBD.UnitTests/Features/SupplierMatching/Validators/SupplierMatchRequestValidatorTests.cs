using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.SupplierMatching;
using ShilpoHubBD.Application.Validators.SupplierMatching;

namespace ShilpoHubBD.UnitTests.Features.SupplierMatching.Validators;

[Trait("Feature", "SupplierMatching")]
[Trait("Layer", "Validator")]
public class SupplierMatchRequestValidatorTests
{
    private readonly SupplierMatchRequestValidator _validator = new();

    [Fact]
    public void Validate_Defaults_HaveNoErrors()
        => _validator.TestValidate(new SupplierMatchRequest()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_ProductKeywordTooLong_FailsOnProductKeyword()
        => _validator.TestValidate(new SupplierMatchRequest { ProductKeyword = new string('a', 201) }).ShouldHaveValidationErrorFor(x => x.ProductKeyword);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_QuantityBelowOne_FailsOnQuantity(int quantity)
        => _validator.TestValidate(new SupplierMatchRequest { Quantity = quantity }).ShouldHaveValidationErrorFor(x => x.Quantity);

    [Fact]
    public void Validate_NullQuantity_HasNoError()
        => _validator.TestValidate(new SupplierMatchRequest { Quantity = null }).ShouldNotHaveValidationErrorFor(x => x.Quantity);

    [Fact]
    public void Validate_NegativeMaxBudgetPerUnit_FailsOnMaxBudgetPerUnit()
        => _validator.TestValidate(new SupplierMatchRequest { MaxBudgetPerUnit = -1 }).ShouldHaveValidationErrorFor(x => x.MaxBudgetPerUnit);

    [Fact]
    public void Validate_MaterialTooLong_FailsOnMaterial()
        => _validator.TestValidate(new SupplierMatchRequest { Material = new string('a', 101) }).ShouldHaveValidationErrorFor(x => x.Material);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_MaxDeliveryDaysBelowOne_FailsOnMaxDeliveryDays(int days)
        => _validator.TestValidate(new SupplierMatchRequest { MaxDeliveryDays = days }).ShouldHaveValidationErrorFor(x => x.MaxDeliveryDays);

    [Theory]
    [InlineData(-1)]
    [InlineData(5.1)]
    public void Validate_MinRatingOutsideZeroToFive_FailsOnMinRating(decimal rating)
        => _validator.TestValidate(new SupplierMatchRequest { MinRating = rating }).ShouldHaveValidationErrorFor(x => x.MinRating);

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Validate_MaxResultsOutsideOneToFifty_FailsOnMaxResults(int maxResults)
        => _validator.TestValidate(new SupplierMatchRequest { MaxResults = maxResults }).ShouldHaveValidationErrorFor(x => x.MaxResults);

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    public void Validate_MaxResultsAtTheBounds_Passes(int maxResults)
        => _validator.TestValidate(new SupplierMatchRequest { MaxResults = maxResults }).ShouldNotHaveValidationErrorFor(x => x.MaxResults);
}
