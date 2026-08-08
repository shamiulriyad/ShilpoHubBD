using FluentValidation;
using ShilpoHubBD.Application.DTOs.Messaging;

namespace ShilpoHubBD.Application.Validators.Messaging;

public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(4000);
    }
}
