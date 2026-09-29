using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Roles;
using ShilpoHubBD.Application.Validators.Roles;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.UnitTests.Features.Auth.Validators;

[Trait("Feature", "Auth")]
[Trait("Layer", "Validator")]
public class AssignRoleRequestValidatorTests
{
    private readonly AssignRoleRequestValidator _validator = new();

    public static TheoryData<string> AllRoles() => new(RoleNames.All);

    [Theory]
    [MemberData(nameof(AllRoles))]
    public void Validate_KnownRoleAndUser_HasNoErrors(string role)
        => _validator.TestValidate(new AssignRoleRequest { UserId = Guid.NewGuid(), Role = role }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_EmptyUserId_FailsOnUserId()
        => _validator.TestValidate(new AssignRoleRequest { UserId = Guid.Empty, Role = RoleNames.Producer })
            .ShouldHaveValidationErrorFor(x => x.UserId);

    [Fact]
    public void Validate_EmptyRole_FailsOnRole()
        => _validator.TestValidate(new AssignRoleRequest { UserId = Guid.NewGuid(), Role = string.Empty })
            .ShouldHaveValidationErrorFor(x => x.Role);

    [Theory]
    [InlineData("Hacker")]
    [InlineData("producer")]
    public void Validate_UnknownOrWrongCaseRole_FailsWithRoleListMessage(string role)
        => _validator.TestValidate(new AssignRoleRequest { UserId = Guid.NewGuid(), Role = role })
            .ShouldHaveValidationErrorFor(x => x.Role)
            .WithErrorMessage($"Role must be one of: {string.Join(", ", RoleNames.All)}.");
}
