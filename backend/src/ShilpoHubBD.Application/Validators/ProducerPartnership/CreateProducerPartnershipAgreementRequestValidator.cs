using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;

namespace ShilpoHubBD.Application.Validators.ProducerPartnership;

public class CreateProducerPartnershipAgreementRequestValidator : AbstractValidator<CreateProducerPartnershipAgreementRequest>
{
    public CreateProducerPartnershipAgreementRequestValidator()
    {
        RuleFor(x => x.ProducerId).NotEmpty();
        RuleFor(x => x.BusinessPartnerId).NotEmpty();
        RuleFor(x => x.WinningBidAmount).GreaterThanOrEqualTo(0).When(x => x.WinningBidAmount.HasValue);
        RuleFor(x => x.PartnershipDurationMonths).GreaterThanOrEqualTo(1).When(x => x.PartnershipDurationMonths.HasValue);
        RuleFor(x => x.AgreementTerms).MaximumLength(4000);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}
