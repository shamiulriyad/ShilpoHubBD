using FluentValidation;
using ShilpoHubBD.Application.DTOs.Assessment;

namespace ShilpoHubBD.Application.Validators.Assessment;

public class CreateExamQuestionOptionRequestValidator : AbstractValidator<CreateExamQuestionOptionRequest>
{
    public CreateExamQuestionOptionRequestValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
    }
}
