using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;

namespace ShilpoHubBD.Application.Validators.ProducerPartnership;

public class DecideProducerPartnershipAuctionParticipantRequestValidator : AbstractValidator<DecideProducerPartnershipAuctionParticipantRequest>
{
    public DecideProducerPartnershipAuctionParticipantRequestValidator()
    {
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
