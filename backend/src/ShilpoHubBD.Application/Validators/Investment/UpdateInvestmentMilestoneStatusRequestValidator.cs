using FluentValidation;
using ShilpoHubBD.Application.DTOs.Investment;

namespace ShilpoHubBD.Application.Validators.Investment;

public class UpdateInvestmentMilestoneStatusRequestValidator : AbstractValidator<UpdateInvestmentMilestoneStatusRequest>
{
    public UpdateInvestmentMilestoneStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
