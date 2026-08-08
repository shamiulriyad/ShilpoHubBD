using FluentValidation;
using ShilpoHubBD.Application.DTOs.Traceability;

namespace ShilpoHubBD.Application.Validators.Traceability;

public class UpdateProductTraceabilityRequestValidator : AbstractValidator<UpdateProductTraceabilityRequest>
{
    public UpdateProductTraceabilityRequestValidator()
    {
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(2000);
        RuleForEach(x => x.MaterialSources).SetValidator(new MaterialSourceInputValidator());
        RuleForEach(x => x.TimelineEvents).SetValidator(new TimelineEventInputValidator());
    }
}
