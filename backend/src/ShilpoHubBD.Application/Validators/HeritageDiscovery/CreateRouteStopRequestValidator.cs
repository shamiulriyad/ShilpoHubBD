using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;

namespace ShilpoHubBD.Application.Validators.HeritageDiscovery;

public class CreateRouteStopRequestValidator : AbstractValidator<CreateRouteStopRequest>
{
    public CreateRouteStopRequestValidator()
    {
        RuleFor(x => x.HeritagePlaceId).NotEmpty();
        RuleFor(x => x.TransportationMode).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
