using FluentValidation;
using ShilpoHubBD.Application.DTOs.AIBusiness;

namespace ShilpoHubBD.Application.Validators.AIBusiness;

public class ProductDescriptionRequestValidator : AbstractValidator<ProductDescriptionRequest>
{
    public ProductDescriptionRequestValidator()
    {
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200).When(x => x.ProductId is null);
        RuleFor(x => x.Tone).MaximumLength(30);
    }
}
