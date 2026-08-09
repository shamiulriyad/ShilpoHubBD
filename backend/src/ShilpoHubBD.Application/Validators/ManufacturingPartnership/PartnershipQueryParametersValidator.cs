using FluentValidation;
using ShilpoHubBD.Application.DTOs.ManufacturingPartnership;

namespace ShilpoHubBD.Application.Validators.ManufacturingPartnership;

public class PartnershipQueryParametersValidator : AbstractValidator<PartnershipQueryParameters>
{
    public PartnershipQueryParametersValidator()
    {
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
