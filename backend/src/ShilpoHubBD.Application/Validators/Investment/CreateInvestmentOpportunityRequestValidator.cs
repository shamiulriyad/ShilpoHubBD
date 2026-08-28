using FluentValidation;
using ShilpoHubBD.Application.DTOs.Investment;

namespace ShilpoHubBD.Application.Validators.Investment;

public class CreateInvestmentOpportunityRequestValidator : AbstractValidator<CreateInvestmentOpportunityRequest>
{
    public CreateInvestmentOpportunityRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ProjectDescription).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.FundingRequirement).GreaterThan(0);
    }
}
