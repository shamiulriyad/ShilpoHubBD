using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Application.Validators.ProducerPartnership;

public class UpdateProducerPartnershipAuctionRequestValidator : AbstractValidator<UpdateProducerPartnershipAuctionRequest>
{
    public UpdateProducerPartnershipAuctionRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(200);
        RuleFor(x => x.Status)
            .Must(s => Enum.TryParse<ProducerPartnershipAuctionStatus>(s, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage("Status must be one of: Draft, Scheduled, RegistrationOpen, Live, Ended, Settled, Cancelled.");
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.EligibilityCriteria).MaximumLength(2000);
        RuleFor(x => x.ParticipationFee).GreaterThanOrEqualTo(0).When(x => x.ParticipationFee.HasValue);
        RuleFor(x => x.DefaultPartnershipDurationMonths).GreaterThanOrEqualTo(1).When(x => x.DefaultPartnershipDurationMonths.HasValue);
        RuleFor(x => x.DefaultRevenueSharePercentage).InclusiveBetween(0, 100).When(x => x.DefaultRevenueSharePercentage.HasValue);
        RuleFor(x => x.SettlementRulesDescription).MaximumLength(4000);
    }
}
