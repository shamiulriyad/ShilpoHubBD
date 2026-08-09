using FluentValidation;
using ShilpoHubBD.Application.DTOs.Quotations;

namespace ShilpoHubBD.Application.Validators.Quotations;

public class QuotationQueryParametersValidator : AbstractValidator<QuotationQueryParameters>
{
    public QuotationQueryParametersValidator()
    {
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
