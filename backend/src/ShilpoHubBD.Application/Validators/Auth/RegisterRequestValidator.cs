using FluentValidation;
using ShilpoHubBD.Application.DTOs.Auth;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Application.Validators.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("ConfirmPassword must match Password.");

        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Roles)
            .NotEmpty().WithMessage("At least one role must be selected.");

        RuleForEach(x => x.Roles)
            .Must(role => RoleNames.SelfRegisterableRoles.Contains(role))
            .WithMessage($"Role must be one of: {string.Join(", ", RoleNames.SelfRegisterableRoles)}.");
    }
}
