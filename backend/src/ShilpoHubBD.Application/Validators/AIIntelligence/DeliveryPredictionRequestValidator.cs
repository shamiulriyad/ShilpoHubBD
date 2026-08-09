using FluentValidation;
using ShilpoHubBD.Application.DTOs.AIIntelligence;

namespace ShilpoHubBD.Application.Validators.AIIntelligence;

public class DeliveryPredictionRequestValidator : AbstractValidator<DeliveryPredictionRequest>
{
    public DeliveryPredictionRequestValidator()
    {
        RuleFor(x => x.ProducerId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(1).When(x => x.Quantity.HasValue);
    }
}
