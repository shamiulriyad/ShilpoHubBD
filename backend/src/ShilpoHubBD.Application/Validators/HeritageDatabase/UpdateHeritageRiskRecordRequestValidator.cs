using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageDatabase;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Application.Validators.HeritageDatabase;

public class UpdateHeritageRiskRecordRequestValidator : AbstractValidator<UpdateHeritageRiskRecordRequest>
{
    public UpdateHeritageRiskRecordRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.CraftName).MaximumLength(150);
        RuleFor(x => x.ContributingFactors).MaximumLength(2000);
        RuleFor(x => x.RecommendedActions).MaximumLength(2000);
        RuleFor(x => x.Source).MaximumLength(300);
        RuleFor(x => x.AffectedArtisanCount).GreaterThanOrEqualTo(0).When(x => x.AffectedArtisanCount.HasValue);
        RuleFor(x => x.AssessmentYear).InclusiveBetween(1800, 2200).When(x => x.AssessmentYear.HasValue);
        RuleFor(x => x.Category)
            .NotEmpty()
            .Must(v => Enum.TryParse<HeritageRiskCategory>(v, true, out _))
            .WithMessage("Category must be a valid heritage risk category.");
        RuleFor(x => x.Level)
            .NotEmpty()
            .Must(v => Enum.TryParse<HeritageRiskLevel>(v, true, out _))
            .WithMessage("Level must be one of: Low, Moderate, High, Critical, Safeguarded.");
    }
}
