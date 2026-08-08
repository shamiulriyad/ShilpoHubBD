using FluentValidation;
using ShilpoHubBD.Application.DTOs.Messaging;

namespace ShilpoHubBD.Application.Validators.Messaging;

public class ConversationQueryParametersValidator : AbstractValidator<ConversationQueryParameters>
{
    public ConversationQueryParametersValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
