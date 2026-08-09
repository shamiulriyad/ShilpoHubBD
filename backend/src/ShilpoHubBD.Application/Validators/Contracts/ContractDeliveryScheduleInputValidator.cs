using FluentValidation;
using ShilpoHubBD.Application.DTOs.Contracts;

namespace ShilpoHubBD.Application.Validators.Contracts;

public class ContractDeliveryScheduleInputValidator : AbstractValidator<ContractDeliveryScheduleInput>
{
    public ContractDeliveryScheduleInputValidator()
    {
        RuleFor(x => x.ScheduledDate).GreaterThan(DateTime.UtcNow.Date);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
