using FluentValidation;
using ShilpoHubBD.Application.DTOs.Commerce;

namespace ShilpoHubBD.Application.Validators.Commerce;

public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
{
    public UpdateCartItemRequestValidator()
    {
        RuleFor(x => x.Quantity).InclusiveBetween(1, 100);
    }
}
