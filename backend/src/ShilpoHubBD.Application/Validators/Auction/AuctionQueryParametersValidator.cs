using FluentValidation;
using ShilpoHubBD.Application.DTOs.Auction;

namespace ShilpoHubBD.Application.Validators.Auction;

public class AuctionQueryParametersValidator : AbstractValidator<AuctionQueryParameters>
{
    public AuctionQueryParametersValidator()
    {
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status is not null);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
