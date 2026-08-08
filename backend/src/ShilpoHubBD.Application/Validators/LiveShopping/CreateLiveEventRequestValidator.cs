using FluentValidation;
using ShilpoHubBD.Application.DTOs.LiveShopping;

namespace ShilpoHubBD.Application.Validators.LiveShopping;

public class CreateLiveEventRequestValidator : AbstractValidator<CreateLiveEventRequest>
{
    public CreateLiveEventRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.ScheduledStartAt).NotEmpty();
    }
}
