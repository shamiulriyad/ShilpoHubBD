using FluentValidation;
using ShilpoHubBD.Application.DTOs.AIIntelligence;

namespace ShilpoHubBD.Application.Validators.AIIntelligence;

public class RiskAssessmentRequestValidator : AbstractValidator<RiskAssessmentRequest>
{
    public RiskAssessmentRequestValidator()
    {
        RuleFor(x => x.ProducerId).NotEmpty();
    }
}
