using FluentValidation;
using ShilpoHubBD.Application.DTOs.AIBusiness;

namespace ShilpoHubBD.Application.Validators.AIBusiness;

public class MaterialForecastRequestValidator : AbstractValidator<MaterialForecastRequest>
{
    public MaterialForecastRequestValidator()
    {
        RuleFor(x => x.UnitsToProduce).GreaterThan(0);
        RuleFor(x => x.MaterialsPerUnit).NotEmpty();

        RuleForEach(x => x.MaterialsPerUnit).ChildRules(material =>
        {
            material.RuleFor(m => m.MaterialName).NotEmpty().MaximumLength(200);
            material.RuleFor(m => m.QuantityPerUnit).GreaterThan(0);
            material.RuleFor(m => m.Unit).NotEmpty().MaximumLength(30);
        });
    }
}
