using FluentValidation;
using ShilpoHubBD.Application.DTOs.Portfolio;

namespace ShilpoHubBD.Application.Validators.Portfolio;

public class SubmitMentorFeedbackRequestValidator : AbstractValidator<SubmitMentorFeedbackRequest>
{
    public SubmitMentorFeedbackRequestValidator()
    {
        RuleFor(x => x.LearnerUserId).NotEmpty();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5).When(x => x.Rating.HasValue);
    }
}
