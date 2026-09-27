using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;

namespace ShilpoHubBD.Application.Validators.ProducerPartnership;

public class PlaceProducerPartnershipAuctionBidRequestValidator : AbstractValidator<PlaceProducerPartnershipAuctionBidRequest>
{
    public PlaceProducerPartnershipAuctionBidRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
