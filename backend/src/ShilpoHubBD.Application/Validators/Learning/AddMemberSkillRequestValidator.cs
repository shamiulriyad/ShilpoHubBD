using FluentValidation;
using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.Validators.Learning;

public class AddMemberSkillRequestValidator : AbstractValidator<AddMemberSkillRequest>
{
    public AddMemberSkillRequestValidator()
    {
        RuleFor(x => x.HeritageSkillId).NotEmpty();
        RuleFor(x => x.Level).IsInEnum();
    }
}
