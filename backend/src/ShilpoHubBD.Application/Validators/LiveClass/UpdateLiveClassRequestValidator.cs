using FluentValidation;
using ShilpoHubBD.Application.DTOs.LiveClass;

namespace ShilpoHubBD.Application.Validators.LiveClass;

public class UpdateLiveClassRequestValidator : AbstractValidator<UpdateLiveClassRequest>
{
    public UpdateLiveClassRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.MeetingUrl).MaximumLength(2000);
        RuleFor(x => x.MaxParticipants).GreaterThan(0).When(x => x.MaxParticipants.HasValue);
        RuleFor(x => x.ScheduledStartAt).GreaterThan(DateTime.UtcNow).WithMessage("Scheduled start must be in the future.");
    }
}
