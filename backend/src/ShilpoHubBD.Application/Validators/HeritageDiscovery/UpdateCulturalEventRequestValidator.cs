using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;

namespace ShilpoHubBD.Application.Validators.HeritageDiscovery;

public class UpdateCulturalEventRequestValidator : AbstractValidator<UpdateCulturalEventRequest>
{
    public UpdateCulturalEventRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DistrictId).NotEmpty();
        RuleFor(x => x.EventDate).NotEmpty();
        RuleFor(x => x.ImageUrl).MaximumLength(1000);
    }
}
