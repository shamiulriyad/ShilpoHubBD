using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class CreatePreservationStrategyRequestValidator : AbstractValidator<CreatePreservationStrategyRequest>
{
    public CreatePreservationStrategyRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.HeritageProblem).NotEmpty().MaximumLength(6000);
        RuleFor(x => x.ProposedSolution).NotEmpty().MaximumLength(6000);
        RuleFor(x => x.ExpectedImpact).MaximumLength(4000);
        RuleFor(x => x.EvidenceReferences).MaximumLength(6000);
        RuleFor(x => x.TargetDate)
            .GreaterThanOrEqualTo(x => x.StartDate!.Value)
            .When(x => x.StartDate.HasValue && x.TargetDate.HasValue)
            .WithMessage("TargetDate cannot be earlier than StartDate.");
    }
}
