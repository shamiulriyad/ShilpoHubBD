using FluentValidation;
using ShilpoHubBD.Application.DTOs.Cms;

namespace ShilpoHubBD.Application.Validators.Cms;

public class UpdateHomepageSectionRequestValidator : AbstractValidator<UpdateHomepageSectionRequest>
{
    public UpdateHomepageSectionRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Subtitle).MaximumLength(500);
        RuleFor(x => x.ImageUrl).MaximumLength(1000);
        RuleFor(x => x.LinkUrl).MaximumLength(1000);
    }
}
