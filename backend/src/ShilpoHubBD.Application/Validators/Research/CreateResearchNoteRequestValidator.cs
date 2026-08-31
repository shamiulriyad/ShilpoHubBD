using FluentValidation;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Validators.Research;

public class CreateResearchNoteRequestValidator : AbstractValidator<CreateResearchNoteRequest>
{
    public CreateResearchNoteRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(16000);
        RuleFor(x => x.Visibility)
            .Must(v => string.IsNullOrWhiteSpace(v) || Enum.TryParse<ResearchNoteVisibility>(v, true, out _))
            .WithMessage("Visibility must be one of: Private, Team.");
    }
}
