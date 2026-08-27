using FluentValidation;
using ShilpoHubBD.Application.DTOs.Research;

namespace ShilpoHubBD.Application.Validators.Research;

public class CreateResearchMilestoneRequestValidator : AbstractValidator<CreateResearchMilestoneRequest>
{
    public CreateResearchMilestoneRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
    }
}
