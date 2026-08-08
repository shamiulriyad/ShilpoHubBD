using FluentValidation;
using ShilpoHubBD.Application.DTOs.Messaging;

namespace ShilpoHubBD.Application.Validators.Messaging;

public class StartConversationRequestValidator : AbstractValidator<StartConversationRequest>
{
    public StartConversationRequestValidator()
    {
        RuleFor(x => x.RecipientId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty().MaximumLength(4000);
    }
}
