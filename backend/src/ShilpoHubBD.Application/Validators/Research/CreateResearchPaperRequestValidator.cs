using FluentValidation;
using ShilpoHubBD.Application.DTOs.Research;

namespace ShilpoHubBD.Application.Validators.Research;

public class CreateResearchPaperRequestValidator : AbstractValidator<CreateResearchPaperRequest>
{
    public CreateResearchPaperRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Abstract).MaximumLength(8000);
        RuleFor(x => x.Authors).MaximumLength(2000);
        RuleFor(x => x.Keywords).MaximumLength(1000);
        RuleFor(x => x.ManuscriptUrl).MaximumLength(2048);
        RuleFor(x => x.TargetVenue).MaximumLength(300);
    }
}
