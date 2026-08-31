using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class CreateInnovationPrototypeRequestValidator : AbstractValidator<CreateInnovationPrototypeRequest>
{
    public CreateInnovationPrototypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(6000);
        RuleFor(x => x.Category).MaximumLength(100);
    }
}
