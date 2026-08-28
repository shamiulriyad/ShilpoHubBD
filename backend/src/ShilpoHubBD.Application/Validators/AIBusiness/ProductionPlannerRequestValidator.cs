using FluentValidation;
using ShilpoHubBD.Application.DTOs.AIBusiness;

namespace ShilpoHubBD.Application.Validators.AIBusiness;

public class ProductionPlannerRequestValidator : AbstractValidator<ProductionPlannerRequest>
{
    public ProductionPlannerRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.TargetQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DailyProductionCapacity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LeadTimeDays).GreaterThanOrEqualTo(0);
    }
}
