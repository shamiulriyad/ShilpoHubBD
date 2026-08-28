using FluentValidation;
using ShilpoHubBD.Application.DTOs.Contracts;

namespace ShilpoHubBD.Application.Validators.Contracts;

public class ContractQueryParametersValidator : AbstractValidator<ContractQueryParameters>
{
    public ContractQueryParametersValidator()
    {
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
