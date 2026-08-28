using FluentValidation;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Validators.Governance;

public class ComputeHeritageIndexRequestValidator : AbstractValidator<ComputeHeritageIndexRequest>
{
    public ComputeHeritageIndexRequestValidator()
    {
        RuleFor(x => x.IndexType)
            .NotEmpty()
            .Must(v => Enum.TryParse<HeritageIndexType>(v, true, out _))
            .WithMessage("IndexType must be one of: HeritageRiskIndex, LivingHeritageIndex, "
                + "CraftHealthScore, VillageSurvivalIndex, YouthParticipation, ClimateRiskAnalysis.");

        RuleFor(x => x.Scope)
            .NotEmpty()
            .Must(v => Enum.TryParse<HeritageIndexScope>(v, true, out _))
            .WithMessage("Scope must be one of: National, District, Village, Craft.");

        RuleFor(x => x.ScopeId)
            .NotNull()
            .When(x => IsScope(x.Scope, HeritageIndexScope.District) || IsScope(x.Scope, HeritageIndexScope.Village))
            .WithMessage("ScopeId is required for District / Village scope.");

        RuleFor(x => x.CraftLabel)
            .NotEmpty()
            .When(x => IsScope(x.Scope, HeritageIndexScope.Craft))
            .WithMessage("CraftLabel is required for Craft scope.");

        RuleFor(x => x.CraftLabel).MaximumLength(150);
        RuleFor(x => x.Notes).MaximumLength(2000);

        RuleFor(x => x.PeriodEnd)
            .GreaterThan(x => x.PeriodStart!.Value)
            .When(x => x.PeriodStart.HasValue && x.PeriodEnd.HasValue)
            .WithMessage("PeriodEnd must be after PeriodStart.");
    }

    private static bool IsScope(string? value, HeritageIndexScope scope)
        => Enum.TryParse<HeritageIndexScope>(value, true, out var parsed) && parsed == scope;
}
