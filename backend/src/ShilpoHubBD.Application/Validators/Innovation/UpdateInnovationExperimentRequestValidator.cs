using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class UpdateInnovationExperimentRequestValidator : AbstractValidator<UpdateInnovationExperimentRequest>
{
    public UpdateInnovationExperimentRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Objective).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Framework).MaximumLength(100);
        RuleFor(x => x.ConfigJson).MaximumLength(16000);
        RuleFor(x => x.ModelType)
            .NotEmpty()
            .Must(t => Enum.TryParse<InnovationModelType>(t, true, out _))
            .WithMessage("ModelType is not a valid model type.");
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Enum.TryParse<InnovationExperimentStatus>(s, true, out _))
            .WithMessage("Status must be one of: Draft, Active, Archived.");
    }
}
