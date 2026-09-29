using FluentValidation;
using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.Validators.Learning;

public class UpdateCourseRequestValidator : AbstractValidator<UpdateCourseRequest>
{
    public UpdateCourseRequestValidator()
    {
        RuleFor(x => x.Price).InclusiveBetween(0, 1000000);
        RuleFor(x => x.DurationDays).InclusiveBetween(1, 365);
        RuleFor(x => x.DaysPerWeek).InclusiveBetween(1, 7);
        RuleFor(x => x.SessionMinutes).InclusiveBetween(15, 480);
        RuleFor(x => x.ClassTime).Matches(@"^([01]\d|2[0-3]):[0-5]\d$");
        RuleFor(x => x.DeliveryMode).Must(x => x is "Online" or "Offline" or "Both");
        RuleFor(x => x.Venue).NotEmpty().MaximumLength(500).When(x => x.DeliveryMode != "Online");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MaxApprentices).GreaterThan(0).When(x => x.MaxApprentices.HasValue);
    }
}
