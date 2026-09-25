using FluentValidation;
using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Application.Validators.Cms;

public class SaveSiteContentItemRequestValidator : AbstractValidator<SaveSiteContentItemRequest>
{
    public SaveSiteContentItemRequestValidator()
    {
        RuleFor(x => x.Group).NotEmpty().Must(g => SiteContentGroups.All.Contains(g)).WithMessage("Invalid Group.");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Subtitle).MaximumLength(300);
        RuleFor(x => x.Body).MaximumLength(4000);
        RuleFor(x => x.LinkUrl).MaximumLength(1000);
        RuleFor(x => x.Extra).MaximumLength(4000);
    }
}
