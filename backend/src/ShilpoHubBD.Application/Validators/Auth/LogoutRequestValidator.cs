using FluentValidation;
using ShilpoHubBD.Application.DTOs.Auth;

namespace ShilpoHubBD.Application.Validators.Auth;

public class LogoutRequestValidator : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
