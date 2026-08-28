using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class CreateTrainingRunRequestValidator : AbstractValidator<CreateTrainingRunRequest>
{
    public CreateTrainingRunRequestValidator()
    {
        RuleFor(x => x.DatasetSnapshotName).MaximumLength(300);
        RuleFor(x => x.HyperparametersJson).MaximumLength(16000);
        RuleFor(x => x.Notes).MaximumLength(4000);
    }
}
