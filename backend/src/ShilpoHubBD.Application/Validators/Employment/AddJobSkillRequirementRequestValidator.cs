using FluentValidation;
using ShilpoHubBD.Application.DTOs.Employment;

namespace ShilpoHubBD.Application.Validators.Employment;

public class AddJobSkillRequirementRequestValidator : AbstractValidator<AddJobSkillRequirementRequest>
{
    public AddJobSkillRequirementRequestValidator()
    {
        RuleFor(x => x.HeritageSkillId).NotEmpty();
        RuleFor(x => x.MinLevel).IsInEnum();
    }
}
