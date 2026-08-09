using FluentValidation;
using ShilpoHubBD.Application.DTOs.Procurement;

namespace ShilpoHubBD.Application.Validators.Procurement;

public class ProcurementItemInputValidator : AbstractValidator<ProcurementItemInput>
{
    public ProcurementItemInputValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(1);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Specifications).MaximumLength(2000);
    }
}
