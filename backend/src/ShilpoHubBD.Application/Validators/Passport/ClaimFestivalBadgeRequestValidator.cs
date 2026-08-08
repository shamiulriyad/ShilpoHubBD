using FluentValidation;
using ShilpoHubBD.Application.DTOs.Passport;

namespace ShilpoHubBD.Application.Validators.Passport;

public class ClaimFestivalBadgeRequestValidator : AbstractValidator<ClaimFestivalBadgeRequest>
{
    public ClaimFestivalBadgeRequestValidator()
    {
        RuleFor(x => x.BadgeId).NotEmpty();
    }
}
