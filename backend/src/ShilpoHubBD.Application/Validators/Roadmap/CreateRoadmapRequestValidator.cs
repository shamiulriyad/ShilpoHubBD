using FluentValidation;
using ShilpoHubBD.Application.DTOs.Roadmap;

namespace ShilpoHubBD.Application.Validators.Roadmap;

public class CreateRoadmapRequestValidator : AbstractValidator<CreateRoadmapRequest>
{
    public CreateRoadmapRequestValidator()
    {
        RuleFor(x => x.Goal).NotEmpty().MaximumLength(1000);
    }
}
