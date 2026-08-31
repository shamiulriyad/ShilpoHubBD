using FluentValidation;
using ShilpoHubBD.Application.DTOs.FieldResearch;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Application.Validators.FieldResearch;

public class CreateSurveyQuestionRequestValidator : AbstractValidator<CreateSurveyQuestionRequest>
{
    public CreateSurveyQuestionRequestValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.HelpText).MaximumLength(1000);
        RuleFor(x => x.OptionsJson).MaximumLength(4000);
        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.QuestionType)
            .NotEmpty()
            .Must(t => Enum.TryParse<SurveyQuestionType>(t, true, out _))
            .WithMessage("QuestionType is not a valid survey question type.");
        RuleFor(x => x.MaxValue)
            .GreaterThanOrEqualTo(x => x.MinValue!.Value)
            .When(x => x.MinValue.HasValue && x.MaxValue.HasValue)
            .WithMessage("MaxValue cannot be less than MinValue.");
    }
}
