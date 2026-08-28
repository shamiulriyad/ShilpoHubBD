using FluentValidation;
using ShilpoHubBD.Application.DTOs.CSRSponsorship;

namespace ShilpoHubBD.Application.Validators.CSRSponsorship;

public class SubmitProposalRequestValidator : AbstractValidator<SubmitProposalRequest>
{
    public SubmitProposalRequestValidator()
    {
        RuleFor(x => x.FundingAmount).GreaterThan(0);
        RuleFor(x => x.ProposalMessage).MaximumLength(2000);
    }
}
