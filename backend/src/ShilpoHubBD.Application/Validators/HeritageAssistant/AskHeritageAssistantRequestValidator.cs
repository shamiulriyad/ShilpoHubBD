using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageAssistant;

namespace ShilpoHubBD.Application.Validators.HeritageAssistant;

public class AskHeritageAssistantRequestValidator : AbstractValidator<AskHeritageAssistantRequest>
{
    public AskHeritageAssistantRequestValidator()
    {
        RuleFor(x => x.Question).NotEmpty().MaximumLength(500);
    }
}
