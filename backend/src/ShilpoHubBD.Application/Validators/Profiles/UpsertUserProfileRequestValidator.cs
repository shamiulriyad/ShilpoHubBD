using FluentValidation;
using ShilpoHubBD.Application.DTOs.Profiles;

namespace ShilpoHubBD.Application.Validators.Profiles;

public class UpsertUserProfileRequestValidator : AbstractValidator<UpsertUserProfileRequest>
{
    public UpsertUserProfileRequestValidator()
    {
        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().Matches(@"^\+?[0-9][0-9 \-]{6,18}$").WithMessage("Enter a valid phone number.");
        RuleFor(x => x.NidNumber).NotEmpty().Matches(@"^(\d{10}|\d{13}|\d{17})$")
            .WithMessage("The NID number must be 10, 13 or 17 digits.");
        RuleFor(x => x.Expertise).MaximumLength(200);
        RuleFor(x => x.AddressLine).NotEmpty().MaximumLength(500);
        RuleFor(x => x.About).MaximumLength(2000);
    }
}
