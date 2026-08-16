using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;

namespace ShilpoHubBD.Application.Validators.HeritageDiscovery;

public class CreateLocalCuisineRequestValidator : AbstractValidator<CreateLocalCuisineRequest>
{
    public CreateLocalCuisineRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.DistrictId).NotEmpty();
        RuleFor(x => x.WhereToTry).MaximumLength(1000);
        RuleFor(x => x.ImageUrl).MaximumLength(1000);
    }
}
