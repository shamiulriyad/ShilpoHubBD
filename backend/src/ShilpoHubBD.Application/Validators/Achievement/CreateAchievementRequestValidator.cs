using FluentValidation;
using ShilpoHubBD.Application.DTOs.Achievement;

namespace ShilpoHubBD.Application.Validators.Achievement;

public class CreateAchievementRequestValidator : AbstractValidator<CreateAchievementRequest>
{
    public CreateAchievementRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.RequiredXp).GreaterThanOrEqualTo(0);
        RuleFor(x => x.XpReward).GreaterThanOrEqualTo(0);
    }
}
