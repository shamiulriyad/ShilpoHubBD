using FluentValidation;
using ShilpoHubBD.Application.DTOs.Marketplace;

namespace ShilpoHubBD.Application.Validators.Marketplace;

public class StoryChapterInputValidator : AbstractValidator<StoryChapterInput>
{
    public StoryChapterInputValidator()
    {
        RuleFor(x => x.Heading).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(4000);
    }
}
