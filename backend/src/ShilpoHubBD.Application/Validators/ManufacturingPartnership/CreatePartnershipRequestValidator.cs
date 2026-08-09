using FluentValidation;
using ShilpoHubBD.Application.DTOs.ManufacturingPartnership;

namespace ShilpoHubBD.Application.Validators.ManufacturingPartnership;

public class CreatePartnershipRequestValidator : AbstractValidator<CreatePartnershipRequest>
{
    public CreatePartnershipRequestValidator()
    {
        RuleFor(x => x.ProducerId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ProductRequirements).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.ManufacturingSpecifications).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(1);
        RuleFor(x => x.TargetUnitPrice).GreaterThanOrEqualTo(0).When(x => x.TargetUnitPrice.HasValue);
        RuleFor(x => x.TimelineStartDate).GreaterThanOrEqualTo(DateTime.UtcNow.Date);
        RuleFor(x => x.TimelineEndDate).GreaterThan(x => x.TimelineStartDate);

        RuleForEach(x => x.Milestones).SetValidator(new MilestoneInputValidator());
    }
}
