using FluentValidation;
using ShilpoHubBD.Application.DTOs.Marketplace;

namespace ShilpoHubBD.Application.Validators.Marketplace;

public class UpdateCraftStoryRequestValidator : AbstractValidator<UpdateCraftStoryRequest>
{
    public UpdateCraftStoryRequestValidator()
    {
        RuleFor(x => x.Origin).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Since).InclusiveBetween(1700, 2100);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Chapters).NotEmpty().WithMessage("At least one chapter is required.");
        RuleForEach(x => x.Chapters).SetValidator(new StoryChapterInputValidator());
    }
}
