using FluentValidation;
using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.Validators.Learning;

public class AddMentorSkillRequestValidator : AbstractValidator<AddMentorSkillRequest>
{
    public AddMentorSkillRequestValidator()
    {
        RuleFor(x => x.HeritageSkillId).NotEmpty();
        RuleFor(x => x.Level).IsInEnum();
    }
}
