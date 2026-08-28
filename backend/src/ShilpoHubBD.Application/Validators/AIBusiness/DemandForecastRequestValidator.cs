using FluentValidation;
using ShilpoHubBD.Application.DTOs.AIBusiness;

namespace ShilpoHubBD.Application.Validators.AIBusiness;

public class DemandForecastRequestValidator : AbstractValidator<DemandForecastRequest>
{
    public DemandForecastRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.HorizonWeeks).InclusiveBetween(1, 26);
    }
}
