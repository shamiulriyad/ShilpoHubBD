using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class CreateStrategyActionRequestValidator : AbstractValidator<CreateStrategyActionRequest>
{
    public CreateStrategyActionRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.StartDate!.Value)
            .When(x => x.StartDate.HasValue && x.DueDate.HasValue)
            .WithMessage("DueDate cannot be earlier than StartDate.");
    }
}
