using FluentValidation;
using ShilpoHubBD.Application.DTOs.Auth;

namespace ShilpoHubBD.Application.Validators.Auth;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
