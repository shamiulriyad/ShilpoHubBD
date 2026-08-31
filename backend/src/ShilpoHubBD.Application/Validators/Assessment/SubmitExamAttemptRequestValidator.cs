using FluentValidation;
using ShilpoHubBD.Application.DTOs.Assessment;

namespace ShilpoHubBD.Application.Validators.Assessment;

public class SubmitExamAttemptRequestValidator : AbstractValidator<SubmitExamAttemptRequest>
{
    public SubmitExamAttemptRequestValidator()
    {
        RuleFor(x => x.Answers).NotNull();
        RuleForEach(x => x.Answers).ChildRules(answer =>
        {
            answer.RuleFor(a => a.QuestionId).NotEmpty();
            answer.RuleFor(a => a.EssayAnswerText).MaximumLength(8000);
        });
    }
}
