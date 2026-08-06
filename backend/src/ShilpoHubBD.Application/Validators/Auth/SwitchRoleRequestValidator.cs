using FluentValidation;
using ShilpoHubBD.Application.DTOs.Auth;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Application.Validators.Auth;

public class SwitchRoleRequestValidator : AbstractValidator<SwitchRoleRequest>
{
    public SwitchRoleRequestValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => RoleNames.All.Contains(role))
            .WithMessage($"Role must be one of: {string.Join(", ", RoleNames.All)}.");
    }
}
