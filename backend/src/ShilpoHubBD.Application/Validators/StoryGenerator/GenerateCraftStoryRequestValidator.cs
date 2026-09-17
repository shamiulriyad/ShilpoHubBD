using FluentValidation;
using ShilpoHubBD.Application.DTOs.StoryGenerator;

namespace ShilpoHubBD.Application.Validators.StoryGenerator;

public class GenerateCraftStoryRequestValidator : AbstractValidator<GenerateCraftStoryRequest>
{
    public GenerateCraftStoryRequestValidator()
    {
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CraftType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ProducerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.VillageOrDistrict).MaximumLength(200);
    }
}
