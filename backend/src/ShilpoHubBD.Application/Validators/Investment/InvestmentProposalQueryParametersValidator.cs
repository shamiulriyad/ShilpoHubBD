using FluentValidation;
using ShilpoHubBD.Application.DTOs.Investment;

namespace ShilpoHubBD.Application.Validators.Investment;

public class InvestmentProposalQueryParametersValidator : AbstractValidator<InvestmentProposalQueryParameters>
{
    public InvestmentProposalQueryParametersValidator()
    {
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
