using FluentValidation;
using ShilpoHubBD.Application.DTOs.Assessment;

namespace ShilpoHubBD.Application.Validators.Assessment;

public class GradeAssignmentSubmissionRequestValidator : AbstractValidator<GradeAssignmentSubmissionRequest>
{
    public GradeAssignmentSubmissionRequestValidator()
    {
        RuleFor(x => x.Score).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Feedback).MaximumLength(2000);
    }
}
