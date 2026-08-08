using FluentValidation;
using ShilpoHubBD.Application.DTOs.CustomOrders;

namespace ShilpoHubBD.Application.Validators.CustomOrders;

public class CreateCustomOrderRequestValidator : AbstractValidator<CreateCustomOrderRequest>
{
    public CreateCustomOrderRequestValidator()
    {
        RuleFor(x => x.ProducerId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Specifications).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Budget).GreaterThan(0).When(x => x.Budget.HasValue);
        RuleFor(x => x.Deadline).GreaterThan(DateTime.UtcNow).When(x => x.Deadline.HasValue)
            .WithMessage("Deadline must be in the future.");
    }
}
