using FluentValidation;
using ShilpoHubBD.Application.DTOs.Marketplace;

namespace ShilpoHubBD.Application.Validators.Marketplace;

public class UpsertProducerStoryRequestValidator : AbstractValidator<UpsertProducerStoryRequest>
{
    public UpsertProducerStoryRequestValidator()
    {
        RuleFor(x => x.Generations).InclusiveBetween(1, 20);
        RuleFor(x => x.FoundingYear).InclusiveBetween(1700, 2100).When(x => x.FoundingYear.HasValue);
        RuleFor(x => x.Quote).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Chapters).NotEmpty().WithMessage("At least one chapter is required.");
        RuleForEach(x => x.Chapters).SetValidator(new StoryChapterInputValidator());
    }
}
