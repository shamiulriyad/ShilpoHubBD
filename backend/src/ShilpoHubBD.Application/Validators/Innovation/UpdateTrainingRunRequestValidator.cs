using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class UpdateTrainingRunRequestValidator : AbstractValidator<UpdateTrainingRunRequest>
{
    public UpdateTrainingRunRequestValidator()
    {
        RuleFor(x => x.DatasetSnapshotName).MaximumLength(300);
        RuleFor(x => x.HyperparametersJson).MaximumLength(16000);
        RuleFor(x => x.MetricsJson).MaximumLength(16000);
        RuleFor(x => x.PrimaryMetricName).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(4000);
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Enum.TryParse<TrainingRunStatus>(s, true, out _))
            .WithMessage("Status must be one of: Pending, Running, Completed, Failed, Cancelled.");
    }
}
