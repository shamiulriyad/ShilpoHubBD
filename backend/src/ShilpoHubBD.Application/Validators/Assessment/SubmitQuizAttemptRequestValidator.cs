using FluentValidation;
using ShilpoHubBD.Application.DTOs.Assessment;

namespace ShilpoHubBD.Application.Validators.Assessment;

public class SubmitQuizAttemptRequestValidator : AbstractValidator<SubmitQuizAttemptRequest>
{
    public SubmitQuizAttemptRequestValidator()
    {
        RuleFor(x => x.Answers).NotNull();
        RuleForEach(x => x.Answers).ChildRules(answer =>
        {
            answer.RuleFor(a => a.QuestionId).NotEmpty();
        });
    }
}
