using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageIdentity;

namespace ShilpoHubBD.Application.Validators.HeritageIdentity;

public class StoryArchiveEntryInputValidator : AbstractValidator<StoryArchiveEntryInput>
{
    public StoryArchiveEntryInputValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.Year).InclusiveBetween(1700, 2100).When(x => x.Year.HasValue);
    }
}
