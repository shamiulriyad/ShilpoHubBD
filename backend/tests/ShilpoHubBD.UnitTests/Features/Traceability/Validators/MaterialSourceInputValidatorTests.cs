using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Traceability;
using ShilpoHubBD.Application.Validators.Traceability;

namespace ShilpoHubBD.UnitTests.Features.Traceability.Validators;

[Trait("Feature", "Traceability")]
[Trait("Layer", "Validator")]
public class MaterialSourceInputValidatorTests
{
    private readonly MaterialSourceInputValidator _validator = new();

    private static MaterialSourceInput Valid() => new() { MaterialName = "Cotton", SourceLocation = "Rangpur", Description = "Local cotton" };

    [Fact]
    public void Validate_ValidInput_HasNoErrors()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_EmptyMaterialName_FailsOnMaterialName()
    {
        var input = Valid();
        input.MaterialName = "";
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.MaterialName);
    }

    [Fact]
    public void Validate_MaterialNameTooLong_FailsOnMaterialName()
    {
        var input = Valid();
        input.MaterialName = new string('a', 201);
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.MaterialName);
    }

    [Fact]
    public void Validate_EmptySourceLocation_FailsOnSourceLocation()
    {
        var input = Valid();
        input.SourceLocation = "";
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.SourceLocation);
    }

    [Fact]
    public void Validate_SourceLocationTooLong_FailsOnSourceLocation()
    {
        var input = Valid();
        input.SourceLocation = new string('a', 201);
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.SourceLocation);
    }

    [Fact]
    public void Validate_EmptyDescription_FailsOnDescription()
    {
        var input = Valid();
        input.Description = "";
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_DescriptionTooLong_FailsOnDescription()
    {
        var input = Valid();
        input.Description = new string('a', 1001);
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.Description);
    }
}
