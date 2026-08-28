using FluentValidation;
using ShilpoHubBD.Application.DTOs.Investment;

namespace ShilpoHubBD.Application.Validators.Investment;

public class InvestmentMilestoneInputValidator : AbstractValidator<InvestmentMilestoneInput>
{
    public InvestmentMilestoneInputValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.DueDate).GreaterThan(DateTime.UtcNow.Date);
    }
}
