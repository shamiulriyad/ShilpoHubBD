using FluentValidation;
using ShilpoHubBD.Application.DTOs.SupplierMatching;

namespace ShilpoHubBD.Application.Validators.SupplierMatching;

public class SupplierMatchRequestValidator : AbstractValidator<SupplierMatchRequest>
{
    public SupplierMatchRequestValidator()
    {
        RuleFor(x => x.ProductKeyword).MaximumLength(200);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(1).When(x => x.Quantity.HasValue);
        RuleFor(x => x.MaxBudgetPerUnit).GreaterThanOrEqualTo(0).When(x => x.MaxBudgetPerUnit.HasValue);
        RuleFor(x => x.Material).MaximumLength(100);
        RuleFor(x => x.MaxDeliveryDays).GreaterThanOrEqualTo(1).When(x => x.MaxDeliveryDays.HasValue);
        RuleFor(x => x.MinRating).InclusiveBetween(0, 5).When(x => x.MinRating.HasValue);
        RuleFor(x => x.MaxResults).InclusiveBetween(1, 50);
    }
}
