using FluentValidation;
using ShilpoHubBD.Application.DTOs.Traceability;

namespace ShilpoHubBD.Application.Validators.Traceability;

public class MaterialSourceInputValidator : AbstractValidator<MaterialSourceInput>
{
    public MaterialSourceInputValidator()
    {
        RuleFor(x => x.MaterialName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SourceLocation).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
    }
}
