using FluentValidation;
using ShilpoHubBD.Application.DTOs.Assessment;

namespace ShilpoHubBD.Application.Validators.Assessment;

public class UpdateAssignmentRequestValidator : AbstractValidator<UpdateAssignmentRequest>
{
    public UpdateAssignmentRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.MaxScore).GreaterThan(0);
    }
}
