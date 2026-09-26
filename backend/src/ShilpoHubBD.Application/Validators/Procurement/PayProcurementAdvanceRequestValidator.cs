using FluentValidation;
using ShilpoHubBD.Application.DTOs.Procurement;

namespace ShilpoHubBD.Application.Validators.Procurement;

public class PayProcurementAdvanceRequestValidator : AbstractValidator<PayProcurementAdvanceRequest>
{
    public PayProcurementAdvanceRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Method).MaximumLength(60);
        RuleFor(x => x.Reference).MaximumLength(120);
    }
}
