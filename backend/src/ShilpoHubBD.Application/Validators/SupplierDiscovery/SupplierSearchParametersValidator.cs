using FluentValidation;
using ShilpoHubBD.Application.DTOs.SupplierDiscovery;

namespace ShilpoHubBD.Application.Validators.SupplierDiscovery;

public class SupplierSearchParametersValidator : AbstractValidator<SupplierSearchParameters>
{
    public SupplierSearchParametersValidator()
    {
        RuleFor(x => x.Search).MaximumLength(200);
        RuleFor(x => x.ProductName).MaximumLength(200);
        RuleFor(x => x.Material).MaximumLength(100);
        RuleFor(x => x.MinRating).InclusiveBetween(0, 5).When(x => x.MinRating.HasValue);
        RuleFor(x => x.MinPrice).GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue);
        RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(0).When(x => x.MaxPrice.HasValue);
        RuleFor(x => x)
            .Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
            .WithMessage("MinPrice must be less than or equal to MaxPrice.")
            .WithName("MinPrice");
        RuleFor(x => x.MinProductionCapacity).GreaterThanOrEqualTo(0).When(x => x.MinProductionCapacity.HasValue);
        RuleFor(x => x.SortBy).IsInEnum();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
