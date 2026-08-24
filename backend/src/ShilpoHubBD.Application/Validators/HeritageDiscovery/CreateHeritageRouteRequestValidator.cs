using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;

namespace ShilpoHubBD.Application.Validators.HeritageDiscovery;

public class CreateHeritageRouteRequestValidator : AbstractValidator<CreateHeritageRouteRequest>
{
    public CreateHeritageRouteRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.EstimatedDurationMinutes).GreaterThanOrEqualTo(0);
    }
}
