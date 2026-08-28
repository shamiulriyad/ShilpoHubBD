using FluentValidation;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Validators.Research;

public class CreateResearchTaskRequestValidator : AbstractValidator<CreateResearchTaskRequest>
{
    public CreateResearchTaskRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Priority)
            .Must(p => string.IsNullOrWhiteSpace(p) || Enum.TryParse<ResearchTaskPriority>(p, true, out _))
            .WithMessage("Priority must be one of: Low, Medium, High, Critical.");
    }
}
