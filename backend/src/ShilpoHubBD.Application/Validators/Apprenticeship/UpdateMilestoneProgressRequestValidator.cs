using FluentValidation;
using ShilpoHubBD.Application.DTOs.Apprenticeship;

namespace ShilpoHubBD.Application.Validators.Apprenticeship;

public class UpdateMilestoneProgressRequestValidator : AbstractValidator<UpdateMilestoneProgressRequest>
{
    public UpdateMilestoneProgressRequestValidator()
    {
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
