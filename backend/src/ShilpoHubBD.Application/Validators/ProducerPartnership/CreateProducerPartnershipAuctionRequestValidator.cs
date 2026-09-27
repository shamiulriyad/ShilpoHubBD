using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;

namespace ShilpoHubBD.Application.Validators.ProducerPartnership;

public class CreateProducerPartnershipAuctionRequestValidator : AbstractValidator<CreateProducerPartnershipAuctionRequest>
{
    public CreateProducerPartnershipAuctionRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AuctionYear).GreaterThanOrEqualTo(2020);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.EligibilityCriteria).MaximumLength(2000);
        RuleFor(x => x.BusinessPartnerEligibilityCriteria).MaximumLength(2000);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(10);
        RuleFor(x => x.ParticipationFee).GreaterThanOrEqualTo(0).When(x => x.ParticipationFee.HasValue);
        RuleFor(x => x.MinimumStartingBid).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinimumBidIncrement).GreaterThan(0);
        RuleFor(x => x.MaxProducersPerBusinessPartner).GreaterThanOrEqualTo(1).When(x => x.MaxProducersPerBusinessPartner.HasValue);
        RuleFor(x => x.AuctionDurationHours).GreaterThanOrEqualTo(1).When(x => x.AuctionDurationHours.HasValue);
        RuleFor(x => x.DefaultPartnershipDurationMonths).GreaterThanOrEqualTo(1);
        RuleFor(x => x.DefaultRevenueSharePercentage).InclusiveBetween(0, 100).When(x => x.DefaultRevenueSharePercentage.HasValue);
        RuleFor(x => x.RegistrationClosesAt).GreaterThan(x => x.RegistrationOpensAt)
            .When(x => x.RegistrationOpensAt.HasValue && x.RegistrationClosesAt.HasValue);
        RuleFor(x => x.BiddingClosesAt).GreaterThan(x => x.BiddingOpensAt)
            .When(x => x.BiddingOpensAt.HasValue && x.BiddingClosesAt.HasValue);
    }
}
