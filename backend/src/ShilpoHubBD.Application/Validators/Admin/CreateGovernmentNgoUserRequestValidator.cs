using FluentValidation;
using ShilpoHubBD.Application.DTOs.Admin;

namespace ShilpoHubBD.Application.Validators.Admin;

public class CreateGovernmentNgoUserRequestValidator : AbstractValidator<CreateGovernmentNgoUserRequest>
{
    public CreateGovernmentNgoUserRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FullName).MaximumLength(200);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
    }
}
