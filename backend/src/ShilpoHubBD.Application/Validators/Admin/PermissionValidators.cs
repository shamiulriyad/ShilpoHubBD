using FluentValidation;
using ShilpoHubBD.Application.DTOs.Admin;

namespace ShilpoHubBD.Application.Validators.Admin;

public class CreatePermissionRequestValidator : AbstractValidator<CreatePermissionRequest>
{
    public CreatePermissionRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Module).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class SyncRolePermissionsRequestValidator : AbstractValidator<SyncRolePermissionsRequest>
{
    public SyncRolePermissionsRequestValidator()
    {
        RuleForEach(x => x.PermissionCodes).NotEmpty().MaximumLength(100);
    }
}
