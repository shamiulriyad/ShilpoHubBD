using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class AddSubmissionTeamMemberRequestValidator : AbstractValidator<AddSubmissionTeamMemberRequest>
{
    public AddSubmissionTeamMemberRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleOnTeam).MaximumLength(120);
    }
}
