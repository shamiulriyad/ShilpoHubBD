using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;

namespace ShilpoHubBD.Application.Validators.ProducerPartnership;

public class AddProducerPartnershipAuctionLotRequestValidator : AbstractValidator<AddProducerPartnershipAuctionLotRequest>
{
    public AddProducerPartnershipAuctionLotRequestValidator()
    {
        RuleFor(x => x.ProducerId).NotEmpty();
    }
}
