using FluentValidation;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Validators.Logistics;

public class ReceiveStockRequestValidator : AbstractValidator<ReceiveStockRequest>
{
    public ReceiveStockRequestValidator()
    {
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(400);
        RuleFor(x => x.UnitOfMeasure).MaximumLength(20);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.BatchNumber).MaximumLength(80);
        RuleFor(x => x.UnitValue).GreaterThanOrEqualTo(0).When(x => x.UnitValue.HasValue);
        RuleFor(x => x.ReferenceType).MaximumLength(40);
        RuleFor(x => x.Reason).MaximumLength(400);
        RuleFor(x => x.Note).MaximumLength(2000);
    }
}

public class IssueStockRequestValidator : AbstractValidator<IssueStockRequest>
{
    public IssueStockRequestValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.ReferenceType).MaximumLength(40);
        RuleFor(x => x.Reason).MaximumLength(400);
        RuleFor(x => x.Note).MaximumLength(2000);
    }
}

public class TransferStockRequestValidator : AbstractValidator<TransferStockRequest>
{
    public TransferStockRequestValidator()
    {
        RuleFor(x => x.ToBinId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Note).MaximumLength(2000);
    }
}

public class AdjustStockRequestValidator : AbstractValidator<AdjustStockRequest>
{
    public AdjustStockRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(400);
        RuleFor(x => x.Note).MaximumLength(2000);
        RuleFor(x => x.NewQuantityOnHand).GreaterThanOrEqualTo(0).When(x => x.NewQuantityOnHand.HasValue);
        RuleFor(x => x.MovementType)
            .Must(v => Enum.TryParse<WarehouseStockMovementType>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.MovementType))
            .WithMessage("MovementType must be one of: Adjustment, StockCount, Damage, Disposal.");
        RuleFor(x => x.Status)
            .Must(v => Enum.TryParse<WarehouseStockItemStatus>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage("Status must be a valid stock item status.");
        RuleFor(x => x)
            .Must(x => x.NewQuantityOnHand.HasValue ^ x.Delta.HasValue)
            .WithMessage("Provide exactly one of NewQuantityOnHand or Delta.");
    }
}

public class ReserveStockRequestValidator : AbstractValidator<ReserveStockRequest>
{
    public ReserveStockRequestValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.ReferenceType).MaximumLength(40);
        RuleFor(x => x.Note).MaximumLength(2000);
    }
}
