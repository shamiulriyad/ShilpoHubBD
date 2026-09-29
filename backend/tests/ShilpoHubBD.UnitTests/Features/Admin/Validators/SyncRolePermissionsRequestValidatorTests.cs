using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.Validators.Admin;

namespace ShilpoHubBD.UnitTests.Features.Admin.Validators;

[Trait("Feature", "Admin")]
[Trait("Layer", "Validator")]
public class SyncRolePermissionsRequestValidatorTests
{
    private readonly SyncRolePermissionsRequestValidator _validator = new();

    [Fact]
    public void Validate_NoCodes_HasNoErrors()
        => _validator.TestValidate(new SyncRolePermissionsRequest { PermissionCodes = new() }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_ValidCodes_HasNoErrors()
        => _validator.TestValidate(new SyncRolePermissionsRequest { PermissionCodes = new() { "users.manage", "products.approve" } })
            .ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_EmptyCode_FailsOnThatItem()
        => _validator.TestValidate(new SyncRolePermissionsRequest { PermissionCodes = new() { "users.manage", "" } })
            .ShouldHaveValidationErrorFor("PermissionCodes[1]");

    [Fact]
    public void Validate_CodeAtMaximumLength_Passes()
        => _validator.TestValidate(new SyncRolePermissionsRequest { PermissionCodes = new() { new string('c', 100) } })
            .ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_CodeOverMaximumLength_FailsOnThatItem()
        => _validator.TestValidate(new SyncRolePermissionsRequest { PermissionCodes = new() { new string('c', 101) } })
            .ShouldHaveValidationErrorFor("PermissionCodes[0]");
}
