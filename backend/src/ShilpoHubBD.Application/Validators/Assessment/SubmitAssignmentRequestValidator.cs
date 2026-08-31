using FluentValidation;
using ShilpoHubBD.Application.DTOs.Assessment;

namespace ShilpoHubBD.Application.Validators.Assessment;

public class SubmitAssignmentRequestValidator : AbstractValidator<SubmitAssignmentRequest>
{
    public SubmitAssignmentRequestValidator()
    {
        RuleFor(x => x.SubmissionText).NotEmpty().MaximumLength(8000);
        RuleFor(x => x.AttachmentUrl).MaximumLength(2000);
    }
}
