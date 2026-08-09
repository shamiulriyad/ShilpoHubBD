using FluentValidation;
using ShilpoHubBD.Application.DTOs.DesignCollaboration;

namespace ShilpoHubBD.Application.Validators.DesignCollaboration;

public class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
{
    public AddCommentRequestValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
    }
}
