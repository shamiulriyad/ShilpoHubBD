using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class UpdateStrategyObjectiveRequestValidator : AbstractValidator<UpdateStrategyObjectiveRequest>
{
    public UpdateStrategyObjectiveRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
    }
}
