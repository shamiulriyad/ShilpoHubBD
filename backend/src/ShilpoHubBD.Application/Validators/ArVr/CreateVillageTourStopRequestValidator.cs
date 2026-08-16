using FluentValidation;
using ShilpoHubBD.Application.DTOs.ArVr;

namespace ShilpoHubBD.Application.Validators.ArVr;

public class CreateVillageTourStopRequestValidator : AbstractValidator<CreateVillageTourStopRequest>
{
    public CreateVillageTourStopRequestValidator()
    {
        RuleFor(x => x.HeritagePlaceId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.MediaUrl).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.MediaType).IsInEnum();
        RuleFor(x => x.ThumbnailUrl).MaximumLength(1000);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
