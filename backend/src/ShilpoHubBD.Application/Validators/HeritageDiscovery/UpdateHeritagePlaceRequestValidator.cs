using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;

namespace ShilpoHubBD.Application.Validators.HeritageDiscovery;

public class UpdateHeritagePlaceRequestValidator : AbstractValidator<UpdateHeritagePlaceRequest>
{
    public UpdateHeritagePlaceRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.PlaceType).IsInEnum();
        RuleFor(x => x.DistrictId).NotEmpty();
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.ImageUrl).MaximumLength(1000);
    }
}
