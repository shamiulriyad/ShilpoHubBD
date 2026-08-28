using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageIdentity;

namespace ShilpoHubBD.Application.Validators.HeritageIdentity;

public class SkillTimelineEntryInputValidator : AbstractValidator<SkillTimelineEntryInput>
{
    public SkillTimelineEntryInputValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Year).InclusiveBetween(1700, 2100);
    }
}
