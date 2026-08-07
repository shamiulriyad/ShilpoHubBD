using FluentValidation;
using ShilpoHubBD.Application.DTOs.Commerce;

namespace ShilpoHubBD.Application.Validators.Commerce;

public class CancelOrderRequestValidator : AbstractValidator<CancelOrderRequest>
{
    public CancelOrderRequestValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
