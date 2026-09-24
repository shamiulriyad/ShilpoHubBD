using FluentValidation;
using ShilpoHubBD.Application.DTOs.Tourism;

namespace ShilpoHubBD.Application.Validators.Tourism;

public class CreateTourismLocationRequestValidator : AbstractValidator<CreateTourismLocationRequest>
{
    public CreateTourismLocationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.DistrictId).NotEmpty();
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).When(x => x.Price.HasValue);
        RuleFor(x => x.EntryFee).GreaterThanOrEqualTo(0).When(x => x.EntryFee.HasValue);
        RuleFor(x => x.OpeningHours).MaximumLength(200);
        RuleFor(x => x.ContactInfo).MaximumLength(300);
        RuleFor(x => x.Facilities).MaximumLength(1000);
        RuleFor(x => x.ImageUrl).MaximumLength(1000);
    }
}
