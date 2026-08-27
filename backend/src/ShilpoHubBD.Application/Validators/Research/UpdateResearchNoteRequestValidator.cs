using FluentValidation;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Validators.Research;

public class UpdateResearchNoteRequestValidator : AbstractValidator<UpdateResearchNoteRequest>
{
    public UpdateResearchNoteRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(16000);
        RuleFor(x => x.Visibility)
            .NotEmpty()
            .Must(v => Enum.TryParse<ResearchNoteVisibility>(v, true, out _))
            .WithMessage("Visibility must be one of: Private, Team.");
    }
}
