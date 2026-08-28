using FluentValidation;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Validators.Research;

public class UpdateResearchTaskStatusRequestValidator : AbstractValidator<UpdateResearchTaskStatusRequest>
{
    public UpdateResearchTaskStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Enum.TryParse<ResearchTaskStatus>(s, true, out _))
            .WithMessage("Status must be one of: Todo, InProgress, Blocked, Done, Cancelled.");
    }
}
