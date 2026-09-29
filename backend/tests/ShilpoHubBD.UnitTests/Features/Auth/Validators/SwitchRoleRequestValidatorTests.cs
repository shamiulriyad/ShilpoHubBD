using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Auth;
using ShilpoHubBD.Application.Validators.Auth;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.UnitTests.Features.Auth.Validators;

[Trait("Feature", "Auth")]
[Trait("Layer", "Validator")]
public class SwitchRoleRequestValidatorTests
{
    private readonly SwitchRoleRequestValidator _validator = new();

    public static TheoryData<string> AllRoles() => new(RoleNames.All);

    [Theory]
    [MemberData(nameof(AllRoles))]
    public void Validate_AnyKnownRoleIncludingSuperAdmin_HasNoErrors(string role)
        => _validator.TestValidate(new SwitchRoleRequest { Role = role }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_EmptyRole_FailsOnRole()
        => _validator.TestValidate(new SwitchRoleRequest { Role = string.Empty }).ShouldHaveValidationErrorFor(x => x.Role);

    [Theory]
    [InlineData("Hacker")]
    [InlineData("superadmin")]
    public void Validate_UnknownOrWrongCaseRole_FailsWithRoleListMessage(string role)
        => _validator.TestValidate(new SwitchRoleRequest { Role = role })
            .ShouldHaveValidationErrorFor(x => x.Role)
            .WithErrorMessage($"Role must be one of: {string.Join(", ", RoleNames.All)}.");
}
