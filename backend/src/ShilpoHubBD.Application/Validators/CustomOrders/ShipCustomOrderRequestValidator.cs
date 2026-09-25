using FluentValidation;
using ShilpoHubBD.Application.DTOs.CustomOrders;

namespace ShilpoHubBD.Application.Validators.CustomOrders;

public class ShipCustomOrderRequestValidator : AbstractValidator<ShipCustomOrderRequest>
{
    public ShipCustomOrderRequestValidator()
    {
        RuleFor(x => x.LogisticsPartnerProfileId).NotEmpty();
        RuleFor(x => x.WeightKg).GreaterThan(0).When(x => x.WeightKg.HasValue);
        RuleFor(x => x.Notes).MaximumLength(300);
    }
}
