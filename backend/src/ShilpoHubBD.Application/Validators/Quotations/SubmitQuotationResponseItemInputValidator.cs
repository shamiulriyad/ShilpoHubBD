using FluentValidation;
using ShilpoHubBD.Application.DTOs.Quotations;

namespace ShilpoHubBD.Application.Validators.Quotations;

public class SubmitQuotationResponseItemInputValidator : AbstractValidator<SubmitQuotationResponseItemInput>
{
    public SubmitQuotationResponseItemInputValidator()
    {
        RuleFor(x => x.QuotationRequestItemId).NotEmpty();
        RuleFor(x => x.QuotedUnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.QuotedQuantity).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
