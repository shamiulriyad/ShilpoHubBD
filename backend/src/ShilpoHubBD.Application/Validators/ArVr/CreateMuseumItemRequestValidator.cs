using FluentValidation;
using ShilpoHubBD.Application.DTOs.ArVr;

namespace ShilpoHubBD.Application.Validators.ArVr;

public class CreateMuseumItemRequestValidator : AbstractValidator<CreateMuseumItemRequest>
{
    public CreateMuseumItemRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Era).MaximumLength(100);
        RuleFor(x => x.CoverImageUrl).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.ModelUrl).MaximumLength(1000);
        RuleFor(x => x.DistrictId).NotEmpty();

        RuleForEach(x => x.Media).ChildRules(media =>
        {
            media.RuleFor(m => m.MediaUrl).NotEmpty().MaximumLength(1000);
            media.RuleFor(m => m.MediaType).IsInEnum();
            media.RuleFor(m => m.Caption).MaximumLength(300);
        });
    }
}
