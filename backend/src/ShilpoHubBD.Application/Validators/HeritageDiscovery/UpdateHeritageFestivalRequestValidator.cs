using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;

namespace ShilpoHubBD.Application.Validators.HeritageDiscovery;

public class UpdateHeritageFestivalRequestValidator : AbstractValidator<UpdateHeritageFestivalRequest>
{
    public UpdateHeritageFestivalRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.DistrictId).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty().GreaterThanOrEqualTo(x => x.StartDate);
        RuleFor(x => x.ImageUrl).MaximumLength(1000);
    }
}
