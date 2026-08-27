using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class CreateSubmissionReviewRequestValidator : AbstractValidator<CreateSubmissionReviewRequest>
{
    public CreateSubmissionReviewRequestValidator()
    {
        RuleFor(x => x.Comments).NotEmpty().MaximumLength(6000);
        RuleFor(x => x.Score).InclusiveBetween(0, 100).When(x => x.Score.HasValue);
        RuleFor(x => x.Decision)
            .NotEmpty()
            .Must(d => Enum.TryParse<SubmissionReviewDecision>(d, true, out _))
            .WithMessage("Decision must be one of: Comment, RequestRevision, Approve, Reject.");
    }
}
