using FluentValidation;
using ShilpoHubBD.Application.DTOs.Quotations;

namespace ShilpoHubBD.Application.Validators.Quotations;

public class SubmitQuotationResponseRequestValidator : AbstractValidator<SubmitQuotationResponseRequest>
{
    public SubmitQuotationResponseRequestValidator()
    {
        RuleFor(x => x.TotalPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item must be quoted.");
        RuleForEach(x => x.Items).SetValidator(new SubmitQuotationResponseItemInputValidator());
    }
}
