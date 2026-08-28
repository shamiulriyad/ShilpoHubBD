using FluentValidation;
using ShilpoHubBD.Application.DTOs.Investment;

namespace ShilpoHubBD.Application.Validators.Investment;

public class InvestmentProposalDecisionRequestValidator : AbstractValidator<InvestmentProposalDecisionRequest>
{
    public InvestmentProposalDecisionRequestValidator()
    {
        RuleFor(x => x.DecisionNotes).MaximumLength(1000);
    }
}
