using FluentValidation;
using ShilpoHubBD.Application.DTOs.TouristBooking;

namespace ShilpoHubBD.Application.Validators.TouristBooking;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.AvailabilitySlotId).NotEmpty();
        RuleFor(x => x.PartySize).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
