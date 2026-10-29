using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProducerBusiness;

namespace ShilpoHubBD.Application.Validators.ProducerBusiness;

public class ShipOrderItemRequestValidator : AbstractValidator<ShipOrderItemRequest>
{
    public ShipOrderItemRequestValidator()
    {
        RuleFor(x => x.Notes).MaximumLength(300);
    }
}
