using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.SupplierDiscovery;
using ShilpoHubBD.Application.Validators.SupplierDiscovery;

namespace ShilpoHubBD.UnitTests.Features.SupplierDiscovery.Validators;

[Trait("Feature", "SupplierDiscovery")]
[Trait("Layer", "Validator")]
public class SupplierSearchParametersValidatorTests
{
    private readonly SupplierSearchParametersValidator _validator = new();

    [Fact]
    public void Validate_Defaults_HaveNoErrors()
        => _validator.TestValidate(new SupplierSearchParameters()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_SearchTooLong_FailsOnSearch()
        => _validator.TestValidate(new SupplierSearchParameters { Search = new string('a', 201) }).ShouldHaveValidationErrorFor(x => x.Search);

    [Fact]
    public void Validate_ProductNameTooLong_FailsOnProductName()
        => _validator.TestValidate(new SupplierSearchParameters { ProductName = new string('a', 201) }).ShouldHaveValidationErrorFor(x => x.ProductName);

    [Fact]
    public void Validate_MaterialTooLong_FailsOnMaterial()
        => _validator.TestValidate(new SupplierSearchParameters { Material = new string('a', 101) }).ShouldHaveValidationErrorFor(x => x.Material);

    [Theory]
    [InlineData(-1)]
    [InlineData(5.1)]
    public void Validate_MinRatingOutsideZeroToFive_FailsOnMinRating(decimal rating)
        => _validator.TestValidate(new SupplierSearchParameters { MinRating = rating }).ShouldHaveValidationErrorFor(x => x.MinRating);

    [Fact]
    public void Validate_NullMinRating_HasNoError()
        => _validator.TestValidate(new SupplierSearchParameters { MinRating = null }).ShouldNotHaveValidationErrorFor(x => x.MinRating);

    [Fact]
    public void Validate_NegativeMinPrice_FailsOnMinPrice()
        => _validator.TestValidate(new SupplierSearchParameters { MinPrice = -1 }).ShouldHaveValidationErrorFor(x => x.MinPrice);

    [Fact]
    public void Validate_NegativeMaxPrice_FailsOnMaxPrice()
        => _validator.TestValidate(new SupplierSearchParameters { MaxPrice = -1 }).ShouldHaveValidationErrorFor(x => x.MaxPrice);

    [Fact]
    public void Validate_MinPriceGreaterThanMaxPrice_FailsOnMinPrice()
        => _validator.TestValidate(new SupplierSearchParameters { MinPrice = 500, MaxPrice = 100 }).ShouldHaveValidationErrorFor("MinPrice");

    [Fact]
    public void Validate_MinPriceLessThanOrEqualToMaxPrice_HasNoError()
        => _validator.TestValidate(new SupplierSearchParameters { MinPrice = 100, MaxPrice = 500 }).ShouldNotHaveValidationErrorFor("MinPrice");

    [Fact]
    public void Validate_NegativeMinProductionCapacity_FailsOnMinProductionCapacity()
        => _validator.TestValidate(new SupplierSearchParameters { MinProductionCapacity = -1 }).ShouldHaveValidationErrorFor(x => x.MinProductionCapacity);

    [Fact]
    public void Validate_InvalidSortBy_FailsOnSortBy()
        => _validator.TestValidate(new SupplierSearchParameters { SortBy = (ShilpoHubBD.Domain.Entities.SupplierDiscovery.SupplierSortOption)999 })
            .ShouldHaveValidationErrorFor(x => x.SortBy);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PageBelowOne_FailsOnPage(int page)
        => _validator.TestValidate(new SupplierSearchParameters { Page = page }).ShouldHaveValidationErrorFor(x => x.Page);

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Validate_PageSizeOutsideOneToFifty_FailsOnPageSize(int pageSize)
        => _validator.TestValidate(new SupplierSearchParameters { PageSize = pageSize }).ShouldHaveValidationErrorFor(x => x.PageSize);
}
