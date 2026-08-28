using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProductDevelopment;

namespace ShilpoHubBD.Application.Validators.ProductDevelopment;

public class UpdateDevelopmentMilestoneStatusRequestValidator : AbstractValidator<UpdateDevelopmentMilestoneStatusRequest>
{
    public UpdateDevelopmentMilestoneStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
