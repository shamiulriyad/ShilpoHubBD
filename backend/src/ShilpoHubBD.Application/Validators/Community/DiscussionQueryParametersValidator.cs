using FluentValidation;
using ShilpoHubBD.Application.DTOs.Community;

namespace ShilpoHubBD.Application.Validators.Community;

public class DiscussionQueryParametersValidator : AbstractValidator<DiscussionQueryParameters>
{
    public DiscussionQueryParametersValidator()
    {
        RuleFor(x => x.Category).MaximumLength(100);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
