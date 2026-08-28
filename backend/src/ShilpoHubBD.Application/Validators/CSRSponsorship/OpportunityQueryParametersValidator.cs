using FluentValidation;
using ShilpoHubBD.Application.DTOs.CSRSponsorship;

namespace ShilpoHubBD.Application.Validators.CSRSponsorship;

public class OpportunityQueryParametersValidator : AbstractValidator<OpportunityQueryParameters>
{
    public OpportunityQueryParametersValidator()
    {
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
