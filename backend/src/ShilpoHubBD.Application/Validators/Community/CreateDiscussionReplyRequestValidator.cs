using FluentValidation;
using ShilpoHubBD.Application.DTOs.Community;

namespace ShilpoHubBD.Application.Validators.Community;

public class CreateDiscussionReplyRequestValidator : AbstractValidator<CreateDiscussionReplyRequest>
{
    public CreateDiscussionReplyRequestValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(2000);
    }
}
