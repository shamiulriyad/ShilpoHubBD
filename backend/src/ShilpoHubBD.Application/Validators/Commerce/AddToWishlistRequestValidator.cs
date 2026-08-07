using FluentValidation;
using ShilpoHubBD.Application.DTOs.Commerce;

namespace ShilpoHubBD.Application.Validators.Commerce;

public class AddToWishlistRequestValidator : AbstractValidator<AddToWishlistRequest>
{
    public AddToWishlistRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
    }
}
