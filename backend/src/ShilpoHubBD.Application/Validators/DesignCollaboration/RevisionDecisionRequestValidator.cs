using FluentValidation;
using ShilpoHubBD.Application.DTOs.DesignCollaboration;
using ShilpoHubBD.Domain.Entities.DesignCollaboration;

namespace ShilpoHubBD.Application.Validators.DesignCollaboration;

public class RevisionDecisionRequestValidator : AbstractValidator<RevisionDecisionRequest>
{
    public RevisionDecisionRequestValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => s is RevisionStatus.Approved or RevisionStatus.Rejected)
            .WithMessage("Status must be Approved or Rejected.");
        RuleFor(x => x.DecisionNotes).MaximumLength(1000);
    }
}
