using FluentValidation;
using ShilpoHubBD.Application.DTOs.Marketplace;

namespace ShilpoHubBD.Application.Validators.Marketplace;

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);

        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.DiscountPrice)
            .LessThan(x => x.Price)
            .When(x => x.DiscountPrice.HasValue)
            .WithMessage("DiscountPrice must be less than Price.");

        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);

        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.DistrictId).NotEmpty();

        RuleFor(x => x.ImageUrls).NotEmpty().WithMessage("At least one product image is required.");
        RuleForEach(x => x.ImageUrls).NotEmpty().MaximumLength(2000);

        RuleFor(x => x.MakingProcessVideoUrl).MaximumLength(2000);
    }
}
