using FluentValidation;
using ShilpoHubBD.Application.DTOs.FieldResearch;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Application.Validators.FieldResearch;

public class UpdateFieldAssignmentRequestValidator : AbstractValidator<UpdateFieldAssignmentRequest>
{
    public UpdateFieldAssignmentRequestValidator()
    {
        RuleFor(x => x.AreaNote).MaximumLength(500);
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(r => Enum.TryParse<FieldAssignmentRole>(r, true, out _))
            .WithMessage("Role must be one of: Collector, Supervisor, Reviewer.");
    }
}
