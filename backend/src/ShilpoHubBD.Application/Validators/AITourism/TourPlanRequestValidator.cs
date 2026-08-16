using FluentValidation;
using ShilpoHubBD.Application.DTOs.AITourism;

namespace ShilpoHubBD.Application.Validators.AITourism;

public class TourPlanRequestValidator : AbstractValidator<TourPlanRequest>
{
    public TourPlanRequestValidator()
    {
        RuleFor(x => x.DurationDays).InclusiveBetween(1, 30);
        RuleFor(x => x.PartySize).InclusiveBetween(1, 100);
    }
}
