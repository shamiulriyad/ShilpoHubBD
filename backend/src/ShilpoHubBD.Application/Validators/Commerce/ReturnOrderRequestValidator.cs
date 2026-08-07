using FluentValidation;
using ShilpoHubBD.Application.DTOs.Commerce;

namespace ShilpoHubBD.Application.Validators.Commerce;

public class ReturnOrderRequestValidator : AbstractValidator<ReturnOrderRequest>
{
    public ReturnOrderRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
