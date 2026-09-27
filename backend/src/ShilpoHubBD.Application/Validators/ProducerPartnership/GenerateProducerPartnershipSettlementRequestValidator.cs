using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;

namespace ShilpoHubBD.Application.Validators.ProducerPartnership;

public class GenerateProducerPartnershipSettlementRequestValidator : AbstractValidator<GenerateProducerPartnershipSettlementRequest>
{
    public GenerateProducerPartnershipSettlementRequestValidator()
    {
        RuleFor(x => x.PeriodEnd).GreaterThan(x => x.PeriodStart);
    }
}
