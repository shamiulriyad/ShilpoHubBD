using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class CreatePrototypeIterationRequestValidator : AbstractValidator<CreatePrototypeIterationRequest>
{
    public CreatePrototypeIterationRequestValidator()
    {
        RuleFor(x => x.Label).MaximumLength(50);
        RuleFor(x => x.ChangeSummary).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ArtifactUrl).MaximumLength(2048);
    }
}
