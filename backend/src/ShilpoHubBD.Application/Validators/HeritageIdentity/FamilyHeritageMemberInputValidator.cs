using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageIdentity;

namespace ShilpoHubBD.Application.Validators.HeritageIdentity;

public class FamilyHeritageMemberInputValidator : AbstractValidator<FamilyHeritageMemberInput>
{
    public FamilyHeritageMemberInputValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Relation).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Generation).InclusiveBetween(1, 20);
        RuleFor(x => x.Role).MaximumLength(150);
        RuleFor(x => x.ActiveYearsRange).MaximumLength(50);
        RuleFor(x => x.Story).MaximumLength(2000);
    }
}
