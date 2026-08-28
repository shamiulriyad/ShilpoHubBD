using FluentValidation;
using ShilpoHubBD.Application.DTOs.Contracts;

namespace ShilpoHubBD.Application.Validators.Contracts;

public class RenewContractRequestValidator : AbstractValidator<RenewContractRequest>
{
    public RenewContractRequestValidator()
    {
        RuleFor(x => x.NewEndDate).GreaterThan(DateTime.UtcNow.Date);
        RuleForEach(x => x.Items).SetValidator(new ContractItemInputValidator());
        RuleForEach(x => x.DeliverySchedules).SetValidator(new ContractDeliveryScheduleInputValidator());
    }
}
