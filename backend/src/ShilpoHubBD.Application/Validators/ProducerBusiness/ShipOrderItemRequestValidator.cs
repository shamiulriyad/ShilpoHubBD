using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProducerBusiness;

namespace ShilpoHubBD.Application.Validators.ProducerBusiness;

public class ShipOrderItemRequestValidator : AbstractValidator<ShipOrderItemRequest>
{
    public ShipOrderItemRequestValidator()
    {
        RuleFor(x => x.TrackingNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Carrier).NotEmpty().MaximumLength(100);
    }
}
