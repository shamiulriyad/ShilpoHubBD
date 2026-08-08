using FluentValidation;
using ShilpoHubBD.Application.DTOs.Auction;

namespace ShilpoHubBD.Application.Validators.Auction;

public class PlaceBidRequestValidator : AbstractValidator<PlaceBidRequest>
{
    public PlaceBidRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
