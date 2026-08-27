using FluentValidation;
using ShilpoHubBD.Application.DTOs.FieldResearch;

namespace ShilpoHubBD.Application.Validators.FieldResearch;

public class ReviewSurveyResponseRequestValidator : AbstractValidator<ReviewSurveyResponseRequest>
{
    private static readonly string[] Allowed =
        { "approve", "approved", "reject", "rejected", "review", "underreview", "under_review" };

    public ReviewSurveyResponseRequestValidator()
    {
        RuleFor(x => x.Decision)
            .NotEmpty()
            .Must(d => Allowed.Contains(d.Trim().ToLowerInvariant()))
            .WithMessage("Decision must be one of: approve, reject, review.");
        RuleFor(x => x.Note).MaximumLength(2000);
    }
}
