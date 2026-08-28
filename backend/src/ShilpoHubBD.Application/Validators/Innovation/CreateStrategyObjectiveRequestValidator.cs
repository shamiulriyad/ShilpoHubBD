using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class CreateStrategyObjectiveRequestValidator : AbstractValidator<CreateStrategyObjectiveRequest>
{
    public CreateStrategyObjectiveRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
    }
}
