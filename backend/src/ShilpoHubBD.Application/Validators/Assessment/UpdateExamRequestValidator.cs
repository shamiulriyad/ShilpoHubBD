using FluentValidation;
using ShilpoHubBD.Application.DTOs.Assessment;

namespace ShilpoHubBD.Application.Validators.Assessment;

public class UpdateExamRequestValidator : AbstractValidator<UpdateExamRequest>
{
    public UpdateExamRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.TimeLimitMinutes).GreaterThan(0).When(x => x.TimeLimitMinutes.HasValue);
        RuleFor(x => x.MaxAttempts).GreaterThan(0).When(x => x.MaxAttempts.HasValue);
        RuleFor(x => x.PassingScorePercentage).InclusiveBetween(0, 100);
    }
}
