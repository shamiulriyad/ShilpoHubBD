using FluentValidation;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Validators.Research;

public class GenerateResearchCitationsRequestValidator : AbstractValidator<GenerateResearchCitationsRequest>
{
    public GenerateResearchCitationsRequestValidator()
    {
        RuleFor(x => x.Title).MaximumLength(200);

        RuleFor(x => x.Style)
            .NotEmpty()
            .Must(s => Enum.TryParse<ResearchCitationStyle>(s, true, out _))
            .WithMessage("Style must be one of: Apa, Mla, Chicago, Ieee, Bibtex.");

        RuleFor(x => x)
            .Must(x => (x.Sources?.Count ?? 0) > 0 || (x.PublicationIds?.Count ?? 0) > 0)
            .WithMessage("Provide at least one citation source or publication id.");

        RuleFor(x => x.Sources.Count).LessThanOrEqualTo(200)
            .WithMessage("At most 200 sources can be submitted at once.");

        RuleForEach(x => x.Sources).ChildRules(source =>
        {
            source.RuleFor(s => s.Title).NotEmpty().MaximumLength(400);
            source.RuleFor(s => s.Authors).MaximumLength(2000);
            source.RuleFor(s => s.Container).MaximumLength(400);
            source.RuleFor(s => s.Doi).MaximumLength(200);
            source.RuleFor(s => s.Url).MaximumLength(2048);
            source.RuleFor(s => s.Year).InclusiveBetween(1000, 2200).When(s => s.Year.HasValue);
        });
    }
}
