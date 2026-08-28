using FluentValidation;
using ShilpoHubBD.Application.DTOs.Assessment;

namespace ShilpoHubBD.Application.Validators.Assessment;

public class EvaluateExamAnswerRequestValidator : AbstractValidator<EvaluateExamAnswerRequest>
{
    public EvaluateExamAnswerRequestValidator()
    {
        RuleFor(x => x.PointsAwarded).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Feedback).MaximumLength(2000);
    }
}
