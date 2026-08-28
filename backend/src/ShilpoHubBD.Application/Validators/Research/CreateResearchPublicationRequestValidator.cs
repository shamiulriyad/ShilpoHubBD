using FluentValidation;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Validators.Research;

public class CreateResearchPublicationRequestValidator : AbstractValidator<CreateResearchPublicationRequest>
{
    public CreateResearchPublicationRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Authors).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Venue).MaximumLength(300);
        RuleFor(x => x.Doi).MaximumLength(200);
        RuleFor(x => x.Url).MaximumLength(2048);
        RuleFor(x => x.Abstract).MaximumLength(8000);
        RuleFor(x => x.Citation).MaximumLength(2000);
        RuleFor(x => x.Type)
            .Must(t => string.IsNullOrWhiteSpace(t) || Enum.TryParse<ResearchPublicationType>(t, true, out _))
            .WithMessage("Type must be a valid publication type.");
    }
}
