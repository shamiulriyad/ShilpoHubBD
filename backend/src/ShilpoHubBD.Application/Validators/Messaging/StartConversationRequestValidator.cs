using FluentValidation;
using ShilpoHubBD.Application.DTOs.Messaging;

namespace ShilpoHubBD.Application.Validators.Messaging;

public class StartConversationRequestValidator : AbstractValidator<StartConversationRequest>
{
    public StartConversationRequestValidator()
    {
        RuleFor(x => x.RecipientId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty().When(x => string.IsNullOrWhiteSpace(x.ImageUrl)).WithMessage("Write a message or attach a picture.").MaximumLength(4000);
        RuleFor(x => x.ImageUrl).MaximumLength(500).Must(u => u is null || u.StartsWith("/uploads/")).WithMessage("Use an image uploaded through the chat image upload.");
    }
}
