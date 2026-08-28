using FluentValidation;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Validators.Research;

public class UpdateResearchProjectStatusRequestValidator : AbstractValidator<UpdateResearchProjectStatusRequest>
{
    public UpdateResearchProjectStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Enum.TryParse<ResearchProjectStatus>(s, true, out _))
            .WithMessage("Status must be one of: Planning, Active, OnHold, Completed, Archived.");
    }
}
