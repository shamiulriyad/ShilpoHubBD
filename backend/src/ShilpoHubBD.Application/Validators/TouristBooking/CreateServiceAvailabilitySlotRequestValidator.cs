using FluentValidation;
using ShilpoHubBD.Application.DTOs.TouristBooking;

namespace ShilpoHubBD.Application.Validators.TouristBooking;

public class CreateServiceAvailabilitySlotRequestValidator : AbstractValidator<CreateServiceAvailabilitySlotRequest>
{
    public CreateServiceAvailabilitySlotRequestValidator()
    {
        RuleFor(x => x.StartAt).NotEmpty();
        RuleFor(x => x.EndAt).NotEmpty().GreaterThan(x => x.StartAt);
        RuleFor(x => x.Capacity).GreaterThan(0).When(x => x.Capacity.HasValue);
    }
}
