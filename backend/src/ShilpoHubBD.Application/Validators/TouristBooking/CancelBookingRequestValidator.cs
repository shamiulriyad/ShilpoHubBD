using FluentValidation;
using ShilpoHubBD.Application.DTOs.TouristBooking;

namespace ShilpoHubBD.Application.Validators.TouristBooking;

public class CancelBookingRequestValidator : AbstractValidator<CancelBookingRequest>
{
    public CancelBookingRequestValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(1000);
    }
}
