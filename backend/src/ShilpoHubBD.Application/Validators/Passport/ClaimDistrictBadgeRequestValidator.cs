using FluentValidation;
using ShilpoHubBD.Application.DTOs.Passport;

namespace ShilpoHubBD.Application.Validators.Passport;

public class ClaimDistrictBadgeRequestValidator : AbstractValidator<ClaimDistrictBadgeRequest>
{
    public ClaimDistrictBadgeRequestValidator()
    {
        RuleFor(x => x.DistrictId).NotEmpty();
    }
}
