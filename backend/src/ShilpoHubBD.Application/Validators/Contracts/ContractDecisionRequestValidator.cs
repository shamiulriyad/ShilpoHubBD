using FluentValidation;
using ShilpoHubBD.Application.DTOs.Contracts;

namespace ShilpoHubBD.Application.Validators.Contracts;

public class ContractDecisionRequestValidator : AbstractValidator<ContractDecisionRequest>
{
    public ContractDecisionRequestValidator()
    {
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
