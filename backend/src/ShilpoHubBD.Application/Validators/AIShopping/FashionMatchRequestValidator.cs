using FluentValidation;
using ShilpoHubBD.Application.DTOs.AIShopping;

namespace ShilpoHubBD.Application.Validators.AIShopping;

public class FashionMatchRequestValidator : AbstractValidator<FashionMatchRequest>
{
    public FashionMatchRequestValidator()
    {
        RuleFor(x => x.ItemDescription).NotEmpty().MaximumLength(200);
    }
}
