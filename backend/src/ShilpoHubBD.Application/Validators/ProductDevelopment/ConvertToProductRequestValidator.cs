using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProductDevelopment;

namespace ShilpoHubBD.Application.Validators.ProductDevelopment;

public class ConvertToProductRequestValidator : AbstractValidator<ConvertToProductRequest>
{
    public ConvertToProductRequestValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.DistrictId).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.InitialStock).GreaterThanOrEqualTo(0);
    }
}
