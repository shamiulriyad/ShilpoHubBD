using FluentValidation;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Validators.Governance;

public class RunPolicySimulationRequestValidator : AbstractValidator<RunPolicySimulationRequest>
{
    public RunPolicySimulationRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(160);

        RuleFor(x => x.SimulationType)
            .NotEmpty()
            .Must(v => Enum.TryParse<PolicySimulationType>(v, true, out _))
            .WithMessage("SimulationType must be one of: GrantProgram, TrainingProgram, TourismCampaign, "
                + "ExportStrategy, EmploymentPrediction.");

        RuleFor(x => x.Scope)
            .NotEmpty()
            .Must(v => Enum.TryParse<HeritageIndexScope>(v, true, out _))
            .WithMessage("Scope must be one of: National, District, Village, Craft.");

        RuleFor(x => x.ScopeId)
            .NotNull()
            .When(x => IsScope(x.Scope, HeritageIndexScope.District) || IsScope(x.Scope, HeritageIndexScope.Village))
            .WithMessage("ScopeId is required for District / Village scope.");

        RuleFor(x => x.HorizonMonths)
            .InclusiveBetween(3, 120)
            .When(x => x.HorizonMonths.HasValue);

        RuleFor(x => x.Budget).GreaterThanOrEqualTo(0).When(x => x.Budget.HasValue);
        RuleFor(x => x.TargetBeneficiaries).GreaterThanOrEqualTo(0).When(x => x.TargetBeneficiaries.HasValue);
        RuleFor(x => x.DurationMonths).InclusiveBetween(1, 120).When(x => x.DurationMonths.HasValue);
        RuleFor(x => x.IntensityPercent).InclusiveBetween(0, 100).When(x => x.IntensityPercent.HasValue);
        RuleFor(x => x.FocusCraft).MaximumLength(150);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }

    private static bool IsScope(string? value, HeritageIndexScope scope)
        => Enum.TryParse<HeritageIndexScope>(value, true, out var parsed) && parsed == scope;
}
