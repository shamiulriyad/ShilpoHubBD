using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class UpdateStrategyActionRequestValidator : AbstractValidator<UpdateStrategyActionRequest>
{
    public UpdateStrategyActionRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Enum.TryParse<StrategyActionStatus>(s, true, out _))
            .WithMessage("Status must be one of: Planned, InProgress, Done, Blocked, Cancelled.");
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.StartDate!.Value)
            .When(x => x.StartDate.HasValue && x.DueDate.HasValue)
            .WithMessage("DueDate cannot be earlier than StartDate.");
    }
}
