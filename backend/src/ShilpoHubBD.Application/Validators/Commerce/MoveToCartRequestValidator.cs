using FluentValidation;
using ShilpoHubBD.Application.DTOs.Commerce;

namespace ShilpoHubBD.Application.Validators.Commerce;

public class MoveToCartRequestValidator : AbstractValidator<MoveToCartRequest>
{
    public MoveToCartRequestValidator()
    {
        RuleFor(x => x.Quantity).InclusiveBetween(1, 100);
    }
}
