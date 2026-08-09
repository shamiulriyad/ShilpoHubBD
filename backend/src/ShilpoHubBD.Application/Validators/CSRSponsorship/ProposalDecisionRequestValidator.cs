using FluentValidation;
using ShilpoHubBD.Application.DTOs.CSRSponsorship;

namespace ShilpoHubBD.Application.Validators.CSRSponsorship;

public class ProposalDecisionRequestValidator : AbstractValidator<ProposalDecisionRequest>
{
    public ProposalDecisionRequestValidator()
    {
        RuleFor(x => x.DecisionNotes).MaximumLength(1000);
    }
}
