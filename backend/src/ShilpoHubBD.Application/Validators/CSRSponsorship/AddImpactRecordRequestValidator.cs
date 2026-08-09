using FluentValidation;
using ShilpoHubBD.Application.DTOs.CSRSponsorship;

namespace ShilpoHubBD.Application.Validators.CSRSponsorship;

public class AddImpactRecordRequestValidator : AbstractValidator<AddImpactRecordRequest>
{
    public AddImpactRecordRequestValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Metric).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Value).GreaterThanOrEqualTo(0);
    }
}
