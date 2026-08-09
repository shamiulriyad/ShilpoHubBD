using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProductDevelopment;

namespace ShilpoHubBD.Application.Validators.ProductDevelopment;

public class DevelopmentMilestoneInputValidator : AbstractValidator<DevelopmentMilestoneInput>
{
    public DevelopmentMilestoneInputValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.DueDate).GreaterThan(DateTime.UtcNow.Date);
    }
}
