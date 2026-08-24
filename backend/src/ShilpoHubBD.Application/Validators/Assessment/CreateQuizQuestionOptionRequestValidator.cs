using FluentValidation;
using ShilpoHubBD.Application.DTOs.Assessment;

namespace ShilpoHubBD.Application.Validators.Assessment;

public class CreateQuizQuestionOptionRequestValidator : AbstractValidator<CreateQuizQuestionOptionRequest>
{
    public CreateQuizQuestionOptionRequestValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
    }
}
