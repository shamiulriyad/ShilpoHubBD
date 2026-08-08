using FluentValidation;
using ShilpoHubBD.Application.DTOs.Traceability;

namespace ShilpoHubBD.Application.Validators.Traceability;

public class TimelineEventInputValidator : AbstractValidator<TimelineEventInput>
{
    public TimelineEventInputValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.EventDate).NotEmpty();
    }
}
