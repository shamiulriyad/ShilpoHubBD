using FluentValidation;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Validators.Research;

public class CreateResearchProjectRequestValidator : AbstractValidator<CreateResearchProjectRequest>
{
    public CreateResearchProjectRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Description).MaximumLength(8000);
        RuleFor(x => x.Discipline).MaximumLength(150);
        RuleFor(x => x.Institution).MaximumLength(200);
        RuleFor(x => x.Visibility)
            .Must(v => string.IsNullOrWhiteSpace(v) || Enum.TryParse<ResearchProjectVisibility>(v, true, out _))
            .WithMessage("Visibility must be one of: Private, Institutional, Public.");
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate!.Value)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date cannot be earlier than the start date.");
    }
}
