using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProducerComparison;

namespace ShilpoHubBD.Application.Validators.ProducerComparison;

public class ProducerComparisonRequestValidator : AbstractValidator<ProducerComparisonRequest>
{
    public ProducerComparisonRequestValidator()
    {
        RuleFor(x => x.ProducerIds)
            .Must(ids => ids.Distinct().Count() >= 2)
            .WithMessage("At least 2 distinct producers are required to compare.");

        RuleFor(x => x.ProducerIds)
            .Must(ids => ids.Distinct().Count() <= 5)
            .WithMessage("At most 5 producers can be compared at once.");
    }
}
