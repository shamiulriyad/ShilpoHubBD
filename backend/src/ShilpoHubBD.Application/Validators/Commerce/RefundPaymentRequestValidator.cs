using FluentValidation;
using ShilpoHubBD.Application.DTOs.Commerce;

namespace ShilpoHubBD.Application.Validators.Commerce;

public class RefundPaymentRequestValidator : AbstractValidator<RefundPaymentRequest>
{
    public RefundPaymentRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).When(x => x.Amount.HasValue);
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
