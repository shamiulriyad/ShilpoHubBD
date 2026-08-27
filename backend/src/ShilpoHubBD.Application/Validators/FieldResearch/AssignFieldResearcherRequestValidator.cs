using FluentValidation;
using ShilpoHubBD.Application.DTOs.FieldResearch;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Application.Validators.FieldResearch;

public class AssignFieldResearcherRequestValidator : AbstractValidator<AssignFieldResearcherRequest>
{
    public AssignFieldResearcherRequestValidator()
    {
        RuleFor(x => x.FieldResearcherUserId).NotEmpty();
        RuleFor(x => x.AreaNote).MaximumLength(500);
        RuleFor(x => x.Role)
            .Must(r => string.IsNullOrWhiteSpace(r) || Enum.TryParse<FieldAssignmentRole>(r, true, out _))
            .WithMessage("Role must be one of: Collector, Supervisor, Reviewer.");
    }
}
