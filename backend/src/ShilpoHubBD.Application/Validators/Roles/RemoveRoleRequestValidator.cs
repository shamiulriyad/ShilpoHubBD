using FluentValidation;
using ShilpoHubBD.Application.DTOs.Roles;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Application.Validators.Roles;

public class RemoveRoleRequestValidator : AbstractValidator<RemoveRoleRequest>
{
    public RemoveRoleRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => RoleNames.All.Contains(role))
            .WithMessage($"Role must be one of: {string.Join(", ", RoleNames.All)}.");
    }
}
