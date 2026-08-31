using FluentValidation;
using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.Validators.Learning;

public class CreateAcademyMemberProfileRequestValidator : AbstractValidator<CreateAcademyMemberProfileRequest>
{
    public CreateAcademyMemberProfileRequestValidator()
    {
        RuleFor(x => x.Role).IsInEnum();
        RuleFor(x => x.Bio).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.LearningPreferences).MaximumLength(1000);
    }
}
