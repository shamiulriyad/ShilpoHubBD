using FluentValidation;
using ShilpoHubBD.Application.DTOs.CSRSponsorship;

namespace ShilpoHubBD.Application.Validators.CSRSponsorship;

public class SponsorshipMilestoneInputValidator : AbstractValidator<SponsorshipMilestoneInput>
{
    public SponsorshipMilestoneInputValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.DueDate).GreaterThan(DateTime.UtcNow.Date);
    }
}
