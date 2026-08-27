using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageDatabase;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Application.Validators.HeritageDatabase;

public class GrantHeritageDatasetAccessRequestValidator : AbstractValidator<GrantHeritageDatasetAccessRequest>
{
    public GrantHeritageDatasetAccessRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.AccessRole)
            .NotEmpty()
            .Must(v => Enum.TryParse<HeritageDatasetAccessRole>(v, true, out _))
            .WithMessage("AccessRole must be one of: Viewer, Analyst, Maintainer.");
    }
}
