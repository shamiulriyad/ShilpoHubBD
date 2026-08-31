using FluentValidation;
using ShilpoHubBD.Application.DTOs.Assessment;

namespace ShilpoHubBD.Application.Validators.Assessment;

public class CreateQuizQuestionRequestValidator : AbstractValidator<CreateQuizQuestionRequest>
{
    public CreateQuizQuestionRequestValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Points).GreaterThan(0);
        RuleFor(x => x.Options).Must(o => o.Count >= 2).WithMessage("A question must have at least 2 options.");
        RuleFor(x => x.Options).Must(o => o.Count(opt => opt.IsCorrect) == 1)
            .WithMessage("A question must have exactly one correct option.");
        RuleForEach(x => x.Options).SetValidator(new CreateQuizQuestionOptionRequestValidator());
    }
}
