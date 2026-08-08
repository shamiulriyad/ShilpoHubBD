using FluentValidation;
using ShilpoHubBD.Application.DTOs.LiveShopping;

namespace ShilpoHubBD.Application.Validators.LiveShopping;

public class AddLiveCommentRequestValidator : AbstractValidator<AddLiveCommentRequest>
{
    public AddLiveCommentRequestValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(1000);
    }
}
