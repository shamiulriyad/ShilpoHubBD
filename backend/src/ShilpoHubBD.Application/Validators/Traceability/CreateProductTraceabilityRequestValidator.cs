using FluentValidation;
using ShilpoHubBD.Application.DTOs.Traceability;

namespace ShilpoHubBD.Application.Validators.Traceability;

public class CreateProductTraceabilityRequestValidator : AbstractValidator<CreateProductTraceabilityRequest>
{
    public CreateProductTraceabilityRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(2000);
        RuleForEach(x => x.MaterialSources).SetValidator(new MaterialSourceInputValidator());
        RuleForEach(x => x.TimelineEvents).SetValidator(new TimelineEventInputValidator());
    }
}
