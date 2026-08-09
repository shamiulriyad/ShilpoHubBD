using FluentValidation;
using ShilpoHubBD.Application.DTOs.ManufacturingPartnership;

namespace ShilpoHubBD.Application.Validators.ManufacturingPartnership;

public class PartnershipResponseRequestValidator : AbstractValidator<PartnershipResponseRequest>
{
    public PartnershipResponseRequestValidator()
    {
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
