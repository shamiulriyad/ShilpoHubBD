using FluentValidation;
using ShilpoHubBD.Application.DTOs.Cms;

namespace ShilpoHubBD.Application.Validators.Cms;

public class UpdateAnnouncementRequestValidator : AbstractValidator<UpdateAnnouncementRequest>
{
    public UpdateAnnouncementRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Severity).IsInEnum();
    }
}
