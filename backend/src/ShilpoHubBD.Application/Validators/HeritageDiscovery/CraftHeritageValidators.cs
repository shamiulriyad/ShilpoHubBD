using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;

namespace ShilpoHubBD.Application.Validators.HeritageDiscovery;

public class SaveCraftHeritageEntryRequestValidator : AbstractValidator<SaveCraftHeritageEntryRequest>
{
    public SaveCraftHeritageEntryRequestValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100).Matches("^[a-zA-Z0-9-]+$").WithMessage("Slug may only contain letters, numbers and hyphens.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Aliases).MaximumLength(500);
        RuleFor(x => x.Region).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(100);
        RuleFor(x => x.GiName).MaximumLength(300);
        RuleFor(x => x.Unesco).MaximumLength(300);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.History).MaximumLength(4000);
        RuleFor(x => x.Materials).MaximumLength(2000);
        RuleFor(x => x.Process).MaximumLength(4000);
        RuleFor(x => x.Products).MaximumLength(2000);
        RuleFor(x => x.Story).MaximumLength(4000);
        RuleFor(x => x.Visit).MaximumLength(2000);
        RuleFor(x => x.Sources).MaximumLength(4000);
    }
}
