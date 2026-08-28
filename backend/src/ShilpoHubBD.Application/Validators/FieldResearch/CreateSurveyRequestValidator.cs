using FluentValidation;
using ShilpoHubBD.Application.DTOs.FieldResearch;

namespace ShilpoHubBD.Application.Validators.FieldResearch;

public class CreateSurveyRequestValidator : AbstractValidator<CreateSurveyRequest>
{
    public CreateSurveyRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Objective).MaximumLength(2000);
        RuleFor(x => x.TargetRegion).MaximumLength(200);
        RuleFor(x => x.Language).MaximumLength(10);
        RuleFor(x => x.ClosesAt)
            .GreaterThanOrEqualTo(x => x.OpensAt!.Value)
            .When(x => x.OpensAt.HasValue && x.ClosesAt.HasValue)
            .WithMessage("ClosesAt cannot be earlier than OpensAt.");
    }
}
