using FluentValidation;
using ShilpoHubBD.Application.DTOs.Sustainability;

namespace ShilpoHubBD.Application.Validators.Sustainability;

public class CreateMaterialRecordRequestValidator : AbstractValidator<CreateMaterialRecordRequest>
{
    public CreateMaterialRecordRequestValidator()
    {
        RuleFor(x => x.MaterialName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.QuantityUsed).GreaterThan(0);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(30);
        RuleFor(x => x.CarbonSavingsPerUnitKg).GreaterThanOrEqualTo(0);
    }
}
