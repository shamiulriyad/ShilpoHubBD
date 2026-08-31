using FluentValidation;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Validators.Research;

public class UpdateResearchPaperRequestValidator : AbstractValidator<UpdateResearchPaperRequest>
{
    public UpdateResearchPaperRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Abstract).MaximumLength(8000);
        RuleFor(x => x.Authors).MaximumLength(2000);
        RuleFor(x => x.Keywords).MaximumLength(1000);
        RuleFor(x => x.ManuscriptUrl).MaximumLength(2048);
        RuleFor(x => x.TargetVenue).MaximumLength(300);
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Enum.TryParse<ResearchPaperStatus>(s, true, out _))
            .WithMessage("Status must be a valid paper status.");
    }
}
