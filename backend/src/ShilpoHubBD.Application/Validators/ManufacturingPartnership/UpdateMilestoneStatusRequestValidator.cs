using FluentValidation;
using ShilpoHubBD.Application.DTOs.ManufacturingPartnership;

namespace ShilpoHubBD.Application.Validators.ManufacturingPartnership;

public class UpdateMilestoneStatusRequestValidator : AbstractValidator<UpdateMilestoneStatusRequest>
{
    public UpdateMilestoneStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
