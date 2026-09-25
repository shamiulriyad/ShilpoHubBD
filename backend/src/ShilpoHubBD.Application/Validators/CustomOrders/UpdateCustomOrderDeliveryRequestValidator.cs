using FluentValidation;
using ShilpoHubBD.Application.DTOs.CustomOrders;

namespace ShilpoHubBD.Application.Validators.CustomOrders;

public class UpdateCustomOrderDeliveryRequestValidator : AbstractValidator<UpdateCustomOrderDeliveryRequest>
{
    public UpdateCustomOrderDeliveryRequestValidator()
    {
        RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RecipientPhone).NotEmpty().MaximumLength(40);
        RuleFor(x => x.ShippingAddressLine).NotEmpty().MaximumLength(500);
    }
}
