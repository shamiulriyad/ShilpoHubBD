using FluentValidation;
using ShilpoHubBD.Application.DTOs.Inventory;

namespace ShilpoHubBD.Application.Validators.Inventory;

public class AdjustStockRequestValidator : AbstractValidator<AdjustStockRequest>
{
    public AdjustStockRequestValidator()
    {
        RuleFor(x => x.ChangeAmount).NotEqual(0).WithMessage("ChangeAmount must not be zero.");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
