using FluentValidation;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Validators.Research;

public class UpdateResearchMilestoneRequestValidator : AbstractValidator<UpdateResearchMilestoneRequest>
{
    public UpdateResearchMilestoneRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Enum.TryParse<ResearchMilestoneStatus>(s, true, out _))
            .WithMessage("Status must be one of: Planned, InProgress, Achieved, Missed.");
    }
}
