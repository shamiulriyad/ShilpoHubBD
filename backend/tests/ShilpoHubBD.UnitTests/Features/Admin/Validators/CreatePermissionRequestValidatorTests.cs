using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.Validators.Admin;

namespace ShilpoHubBD.UnitTests.Features.Admin.Validators;

[Trait("Feature", "Admin")]
[Trait("Layer", "Validator")]
public class CreatePermissionRequestValidatorTests
{
    private readonly CreatePermissionRequestValidator _validator = new();

    private static CreatePermissionRequest Valid() => new() { Code = "users.manage", Name = "Manage users", Module = "Users" };

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_NoDescription_HasNoErrors()
    {
        var request = Valid();
        request.Description = null;

        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_CodeMissing_FailsOnCode(string code)
    {
        var request = Valid();
        request.Code = code;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Validate_CodeAtMaximumLength_Passes()
    {
        var request = Valid();
        request.Code = new string('c', 100);

        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Validate_CodeOverMaximumLength_FailsOnCode()
    {
        var request = Valid();
        request.Code = new string('c', 101);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_NameMissing_FailsOnName(string name)
    {
        var request = Valid();
        request.Name = name;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameOverMaximumLength_FailsOnName()
    {
        var request = Valid();
        request.Name = new string('n', 151);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ModuleMissing_FailsOnModule(string module)
    {
        var request = Valid();
        request.Module = module;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Module);
    }

    [Fact]
    public void Validate_ModuleOverMaximumLength_FailsOnModule()
    {
        var request = Valid();
        request.Module = new string('m', 61);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Module);
    }

    [Fact]
    public void Validate_DescriptionOverMaximumLength_FailsOnDescription()
    {
        var request = Valid();
        request.Description = new string('d', 501);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_DescriptionAtMaximumLength_Passes()
    {
        var request = Valid();
        request.Description = new string('d', 500);

        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}
