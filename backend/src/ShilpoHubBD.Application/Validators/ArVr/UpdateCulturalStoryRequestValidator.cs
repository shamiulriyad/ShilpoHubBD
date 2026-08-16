using FluentValidation;
using ShilpoHubBD.Application.DTOs.ArVr;

namespace ShilpoHubBD.Application.Validators.ArVr;

public class UpdateCulturalStoryRequestValidator : AbstractValidator<UpdateCulturalStoryRequest>
{
    public UpdateCulturalStoryRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.CoverImageUrl).MaximumLength(1000);

        RuleForEach(x => x.Chapters).ChildRules(chapter =>
        {
            chapter.RuleFor(c => c.Heading).NotEmpty().MaximumLength(200);
            chapter.RuleFor(c => c.Body).NotEmpty().MaximumLength(4000);
            chapter.RuleFor(c => c.MediaUrl).MaximumLength(1000);
            chapter.RuleFor(c => c.MediaType).IsInEnum().When(c => c.MediaType.HasValue);
        });
    }
}
