using FluentValidation;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Validators.Research;

public class AddResearchProjectMemberRequestValidator : AbstractValidator<AddResearchProjectMemberRequest>
{
    public AddResearchProjectMemberRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(r => Enum.TryParse<ResearchRole>(r, true, out _))
            .WithMessage("Role must be one of: Viewer, Contributor, Researcher, Admin.");
    }
}
