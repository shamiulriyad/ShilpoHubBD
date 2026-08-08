using FluentValidation;
using ShilpoHubBD.Application.DTOs.LiveShopping;

namespace ShilpoHubBD.Application.Validators.LiveShopping;

public class AddLiveReactionRequestValidator : AbstractValidator<AddLiveReactionRequest>
{
    public AddLiveReactionRequestValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
    }
}
