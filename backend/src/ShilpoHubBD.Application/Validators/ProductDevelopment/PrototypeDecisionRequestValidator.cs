using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProductDevelopment;
using ShilpoHubBD.Domain.Entities.ProductDevelopment;

namespace ShilpoHubBD.Application.Validators.ProductDevelopment;

public class PrototypeDecisionRequestValidator : AbstractValidator<PrototypeDecisionRequest>
{
    public PrototypeDecisionRequestValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => s is PrototypeStatus.Approved or PrototypeStatus.Rejected)
            .WithMessage("Status must be Approved or Rejected.");
        RuleFor(x => x.DecisionNotes).MaximumLength(1000);
    }
}
