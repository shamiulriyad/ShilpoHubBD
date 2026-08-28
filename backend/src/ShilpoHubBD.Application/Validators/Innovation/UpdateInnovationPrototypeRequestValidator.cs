using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class UpdateInnovationPrototypeRequestValidator : AbstractValidator<UpdateInnovationPrototypeRequest>
{
    public UpdateInnovationPrototypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(6000);
        RuleFor(x => x.Category).MaximumLength(100);
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Enum.TryParse<InnovationPrototypeStatus>(s, true, out _))
            .WithMessage("Status is not a valid prototype status.");
    }
}
