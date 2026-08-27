using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class CreateExperimentVersionRequestValidator : AbstractValidator<CreateExperimentVersionRequest>
{
    public CreateExperimentVersionRequestValidator()
    {
        RuleFor(x => x.Label).MaximumLength(50);
        RuleFor(x => x.Notes).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ConfigJson).NotEmpty().MaximumLength(16000);
        RuleFor(x => x.Framework).MaximumLength(100);
        RuleFor(x => x.ArtifactUrl).MaximumLength(2048);
    }
}
