using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class UpdatePreservationStrategyRequestValidator : AbstractValidator<UpdatePreservationStrategyRequest>
{
    public UpdatePreservationStrategyRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.HeritageProblem).NotEmpty().MaximumLength(6000);
        RuleFor(x => x.ProposedSolution).NotEmpty().MaximumLength(6000);
        RuleFor(x => x.ExpectedImpact).MaximumLength(4000);
        RuleFor(x => x.EvidenceReferences).MaximumLength(6000);
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Enum.TryParse<PreservationStrategyStatus>(s, true, out _))
            .WithMessage("Status is not a valid preservation strategy status.");
        RuleFor(x => x.TargetDate)
            .GreaterThanOrEqualTo(x => x.StartDate!.Value)
            .When(x => x.StartDate.HasValue && x.TargetDate.HasValue)
            .WithMessage("TargetDate cannot be earlier than StartDate.");
    }
}
