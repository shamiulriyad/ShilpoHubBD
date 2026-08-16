using FluentValidation;
using ShilpoHubBD.Application.DTOs.TouristBooking;

namespace ShilpoHubBD.Application.Validators.TouristBooking;

public class CreateTouristServiceRequestValidator : AbstractValidator<CreateTouristServiceRequest>
{
    public CreateTouristServiceRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DurationMinutes).GreaterThan(0).When(x => x.DurationMinutes.HasValue);
        RuleFor(x => x.DefaultCapacity).GreaterThan(0);
        RuleFor(x => x.Location).MaximumLength(300);
        RuleFor(x => x.ImageUrl).MaximumLength(1000);
        RuleFor(x => x.DistrictId).NotEmpty();
    }
}
