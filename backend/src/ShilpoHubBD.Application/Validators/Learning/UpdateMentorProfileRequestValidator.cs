using FluentValidation;
using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.Validators.Learning;

public class UpdateMentorProfileRequestValidator : AbstractValidator<UpdateMentorProfileRequest>
{
    public UpdateMentorProfileRequestValidator()
    {
        RuleFor(x => x.Bio).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Expertise).NotEmpty().MaximumLength(500);
        RuleFor(x => x.YearsOfExperience).GreaterThanOrEqualTo(0);
    }
}
