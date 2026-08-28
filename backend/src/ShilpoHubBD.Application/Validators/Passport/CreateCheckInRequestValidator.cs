using FluentValidation;
using ShilpoHubBD.Application.DTOs.Passport;

namespace ShilpoHubBD.Application.Validators.Passport;

public class CreateCheckInRequestValidator : AbstractValidator<CreateCheckInRequest>
{
    public CreateCheckInRequestValidator()
    {
        RuleFor(x => x.HeritagePlaceId).NotEmpty();
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
    }
}
