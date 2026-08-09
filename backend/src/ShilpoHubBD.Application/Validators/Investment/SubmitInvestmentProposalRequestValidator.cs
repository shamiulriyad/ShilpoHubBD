using FluentValidation;
using ShilpoHubBD.Application.DTOs.Investment;

namespace ShilpoHubBD.Application.Validators.Investment;

public class SubmitInvestmentProposalRequestValidator : AbstractValidator<SubmitInvestmentProposalRequest>
{
    public SubmitInvestmentProposalRequestValidator()
    {
        RuleFor(x => x.InvestmentAmount).GreaterThan(0);
        RuleFor(x => x.ProposalMessage).MaximumLength(2000);
    }
}
