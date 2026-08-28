using FluentValidation;
using ShilpoHubBD.Application.DTOs.Assessment;
using ShilpoHubBD.Domain.Entities.Assessment;

namespace ShilpoHubBD.Application.Validators.Assessment;

public class UpdateExamQuestionRequestValidator : AbstractValidator<UpdateExamQuestionRequest>
{
    public UpdateExamQuestionRequestValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Points).GreaterThan(0);

        When(x => x.QuestionType == QuestionType.MultipleChoice, () =>
        {
            RuleFor(x => x.Options).Must(o => o.Count >= 2)
                .WithMessage("A multiple-choice question must have at least 2 options.");
            RuleFor(x => x.Options).Must(o => o.Count(opt => opt.IsCorrect) == 1)
                .WithMessage("A multiple-choice question must have exactly one correct option.");
            RuleForEach(x => x.Options).SetValidator(new CreateExamQuestionOptionRequestValidator());
        });
    }
}
