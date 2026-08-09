using FluentValidation;
using ShilpoHubBD.Application.DTOs.ManufacturingPartnership;

namespace ShilpoHubBD.Application.Validators.ManufacturingPartnership;

public class MilestoneInputValidator : AbstractValidator<MilestoneInput>
{
    public MilestoneInputValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.DueDate).GreaterThan(DateTime.UtcNow.Date);
    }
}
