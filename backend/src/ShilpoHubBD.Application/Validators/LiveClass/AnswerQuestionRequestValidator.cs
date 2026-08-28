using FluentValidation;
using ShilpoHubBD.Application.DTOs.LiveClass;

namespace ShilpoHubBD.Application.Validators.LiveClass;

public class AnswerQuestionRequestValidator : AbstractValidator<AnswerQuestionRequest>
{
    public AnswerQuestionRequestValidator()
    {
        RuleFor(x => x.AnswerBody).NotEmpty().MaximumLength(2000);
    }
}
