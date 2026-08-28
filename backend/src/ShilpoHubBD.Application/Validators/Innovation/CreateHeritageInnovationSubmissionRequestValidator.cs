using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class CreateHeritageInnovationSubmissionRequestValidator : AbstractValidator<CreateHeritageInnovationSubmissionRequest>
{
    public CreateHeritageInnovationSubmissionRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Problem).NotEmpty().MaximumLength(6000);
        RuleFor(x => x.Solution).NotEmpty().MaximumLength(6000);
        RuleFor(x => x.ResearchEvidence).MaximumLength(6000);
    }
}
