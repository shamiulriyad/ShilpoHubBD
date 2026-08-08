using FluentValidation;
using ShilpoHubBD.Application.DTOs.CustomOrders;

namespace ShilpoHubBD.Application.Validators.CustomOrders;

public class RespondToCustomOrderRequestValidator : AbstractValidator<RespondToCustomOrderRequest>
{
    public RespondToCustomOrderRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.QuotedPrice).GreaterThan(0).When(x => x.QuotedPrice.HasValue);
        RuleFor(x => x.ResponseMessage).MaximumLength(2000);
    }
}
