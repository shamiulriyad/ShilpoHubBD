using FluentValidation;
using ShilpoHubBD.Application.DTOs.AIIntelligence;

namespace ShilpoHubBD.Application.Validators.AIIntelligence;

public class PriceForecastRequestValidator : AbstractValidator<PriceForecastRequest>
{
    public PriceForecastRequestValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.HorizonMonths).InclusiveBetween(1, 12);
    }
}
