using FluentValidation;
using ShilpoHubBD.Application.DTOs.AITourism;

namespace ShilpoHubBD.Application.Validators.AITourism;

public class RouteOptimizationRequestValidator : AbstractValidator<RouteOptimizationRequest>
{
    public RouteOptimizationRequestValidator()
    {
        RuleFor(x => x.PlaceIds).NotEmpty();
        RuleForEach(x => x.PlaceIds).NotEmpty();
        RuleFor(x => x.StartLatitude).InclusiveBetween(-90, 90).When(x => x.StartLatitude.HasValue);
        RuleFor(x => x.StartLongitude).InclusiveBetween(-180, 180).When(x => x.StartLongitude.HasValue);
    }
}
