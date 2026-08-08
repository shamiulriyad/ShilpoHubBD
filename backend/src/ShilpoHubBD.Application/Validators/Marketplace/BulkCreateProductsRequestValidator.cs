using FluentValidation;
using ShilpoHubBD.Application.DTOs.Marketplace;

namespace ShilpoHubBD.Application.Validators.Marketplace;

public class BulkCreateProductsRequestValidator : AbstractValidator<BulkCreateProductsRequest>
{
    public BulkCreateProductsRequestValidator()
    {
        RuleFor(x => x.Products).NotEmpty().WithMessage("At least one product is required.");
        RuleFor(x => x.Products).Must(p => p.Count <= 100).WithMessage("A maximum of 100 products can be uploaded at once.");
        RuleForEach(x => x.Products).SetValidator(new CreateProductRequestValidator());
    }
}
