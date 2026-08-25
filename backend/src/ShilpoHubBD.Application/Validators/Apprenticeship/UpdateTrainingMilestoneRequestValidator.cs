using FluentValidation;
using ShilpoHubBD.Application.DTOs.Apprenticeship;

namespace ShilpoHubBD.Application.Validators.Apprenticeship;

public class UpdateTrainingMilestoneRequestValidator : AbstractValidator<UpdateTrainingMilestoneRequest>
{
    public UpdateTrainingMilestoneRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
