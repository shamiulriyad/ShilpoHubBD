using FluentValidation;
using ShilpoHubBD.Application.DTOs.AIIntelligence;

namespace ShilpoHubBD.Application.Validators.AIIntelligence;

public class QualityPredictionRequestValidator : AbstractValidator<QualityPredictionRequest>
{
    public QualityPredictionRequestValidator()
    {
        RuleFor(x => x.ProducerId).NotEmpty();
    }
}
