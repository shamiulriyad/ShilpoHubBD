using FluentValidation;
using ShilpoHubBD.Application.DTOs.Community;

namespace ShilpoHubBD.Application.Validators.Community;

public class CreateDiscussionThreadRequestValidator : AbstractValidator<CreateDiscussionThreadRequest>
{
    public CreateDiscussionThreadRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(4000);
    }
}
