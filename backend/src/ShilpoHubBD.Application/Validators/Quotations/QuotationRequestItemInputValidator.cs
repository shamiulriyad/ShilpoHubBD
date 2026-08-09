using FluentValidation;
using ShilpoHubBD.Application.DTOs.Quotations;

namespace ShilpoHubBD.Application.Validators.Quotations;

public class QuotationRequestItemInputValidator : AbstractValidator<QuotationRequestItemInput>
{
    public QuotationRequestItemInputValidator()
    {
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(1);
        RuleFor(x => x.TargetPrice).GreaterThanOrEqualTo(0).When(x => x.TargetPrice.HasValue);
        RuleFor(x => x.Specifications).MaximumLength(2000);
    }
}
