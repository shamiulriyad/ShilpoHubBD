using FluentValidation;
using ShilpoHubBD.Application.DTOs.LiveShopping;

namespace ShilpoHubBD.Application.Validators.LiveShopping;

public class LiveEventQueryParametersValidator : AbstractValidator<LiveEventQueryParameters>
{
    public LiveEventQueryParametersValidator()
    {
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status is not null);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
