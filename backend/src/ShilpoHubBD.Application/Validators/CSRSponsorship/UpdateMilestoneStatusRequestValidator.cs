using FluentValidation;
using ShilpoHubBD.Application.DTOs.CSRSponsorship;

namespace ShilpoHubBD.Application.Validators.CSRSponsorship;

public class UpdateMilestoneStatusRequestValidator : AbstractValidator<UpdateMilestoneStatusRequest>
{
    public UpdateMilestoneStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
