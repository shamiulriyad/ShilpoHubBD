using FluentValidation;
using ShilpoHubBD.Application.DTOs.LiveClass;

namespace ShilpoHubBD.Application.Validators.LiveClass;

public class AskQuestionRequestValidator : AbstractValidator<AskQuestionRequest>
{
    public AskQuestionRequestValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(1000);
    }
}
