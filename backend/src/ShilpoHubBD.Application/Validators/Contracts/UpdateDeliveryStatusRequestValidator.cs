using FluentValidation;
using ShilpoHubBD.Application.DTOs.Contracts;

namespace ShilpoHubBD.Application.Validators.Contracts;

public class UpdateDeliveryStatusRequestValidator : AbstractValidator<UpdateDeliveryStatusRequest>
{
    public UpdateDeliveryStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
