using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Application.Validators.ProducerPartnership;

public class UpdateProducerPartnershipAgreementTermsRequestValidator : AbstractValidator<UpdateProducerPartnershipAgreementTermsRequest>
{
    public UpdateProducerPartnershipAgreementTermsRequestValidator()
    {
        RuleFor(x => x.PartnershipDurationMonths).GreaterThanOrEqualTo(1).When(x => x.PartnershipDurationMonths.HasValue);
        RuleFor(x => x.ProducerSharePercentage).InclusiveBetween(0, 100).When(x => x.ProducerSharePercentage.HasValue);
        RuleFor(x => x.BusinessPartnerSharePercentage).InclusiveBetween(0, 100).When(x => x.BusinessPartnerSharePercentage.HasValue);
        RuleFor(x => x.PlatformFeePercentage).InclusiveBetween(0, 100).When(x => x.PlatformFeePercentage.HasValue);
        RuleFor(x => x.SettlementFrequency)
            .Must(s => Enum.TryParse<ProducerPartnershipSettlementFrequency>(s, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.SettlementFrequency))
            .WithMessage("Settlement frequency must be one of: Monthly, Quarterly, Biannual, Custom.");
        RuleFor(x => x.CustomSettlementPeriodDays).GreaterThanOrEqualTo(1).When(x => x.CustomSettlementPeriodDays.HasValue);
        RuleFor(x => x.MinimumSettlementAmount).GreaterThanOrEqualTo(0).When(x => x.MinimumSettlementAmount.HasValue);
        RuleFor(x => x.AgreementTerms).MaximumLength(4000);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}
