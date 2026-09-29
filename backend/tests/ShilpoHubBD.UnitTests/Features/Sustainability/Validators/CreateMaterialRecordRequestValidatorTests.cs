using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Sustainability;
using ShilpoHubBD.Application.Validators.Sustainability;

namespace ShilpoHubBD.UnitTests.Features.Sustainability.Validators;

[Trait("Feature", "Sustainability")]
[Trait("Layer", "Validator")]
public class CreateMaterialRecordRequestValidatorTests
{
    private readonly CreateMaterialRecordRequestValidator _validator = new();

    private static CreateMaterialRecordRequest Valid() => new() { MaterialName = "Cotton", QuantityUsed = 2, Unit = "kg", CarbonSavingsPerUnitKg = 1 };

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
    public void Validate_MaterialNameTooLong_FailsOnMaterialName()
    {
        var request = Valid();
        request.MaterialName = new string('a', 201);
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.MaterialName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_QuantityUsedNotGreaterThanZero_FailsOnQuantityUsed(decimal quantity)
    {
        var request = Valid();
        request.QuantityUsed = quantity;
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.QuantityUsed);
    }

    [Fact]
    public void Validate_EmptyUnit_FailsOnUnit()
    {
        var request = Valid();
        request.Unit = "";
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Unit);
    }

    [Fact]
    public void Validate_UnitTooLong_FailsOnUnit()
    {
        var request = Valid();
        request.Unit = new string('a', 31);
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Unit);
    }

    [Fact]
    public void Validate_NegativeCarbonSavings_FailsOnCarbonSavingsPerUnitKg()
    {
        var request = Valid();
        request.CarbonSavingsPerUnitKg = -1;
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.CarbonSavingsPerUnitKg);
    }

    [Fact]
    public void Validate_ZeroCarbonSavings_HasNoError()
    {
        var request = Valid();
        request.CarbonSavingsPerUnitKg = 0;
        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.CarbonSavingsPerUnitKg);
    }
}
