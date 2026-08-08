using FluentValidation;
using ShilpoHubBD.Application.DTOs.LiveShopping;

namespace ShilpoHubBD.Application.Validators.LiveShopping;

public class BuyDuringLiveRequestValidator : AbstractValidator<BuyDuringLiveRequest>
{
    public BuyDuringLiveRequestValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
