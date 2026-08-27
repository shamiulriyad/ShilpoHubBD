using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class CreatePrototypeTestRunRequestValidator : AbstractValidator<CreatePrototypeTestRunRequest>
{
    public CreatePrototypeTestRunRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Environment).MaximumLength(300);
        RuleFor(x => x.Results.Count).LessThanOrEqualTo(1000);
        RuleForEach(x => x.Results).ChildRules(r =>
        {
            r.RuleFor(v => v.CaseTitle).MaximumLength(300);
            r.RuleFor(v => v.ActualResult).MaximumLength(4000);
            r.RuleFor(v => v.Notes).MaximumLength(2000);
            r.RuleFor(v => v.Outcome)
                .NotEmpty()
                .Must(o => Enum.TryParse<TestResultOutcome>(o, true, out _))
                .WithMessage("Outcome must be one of: Pass, Fail, Blocked, Skipped.");
        });
    }
}
