using FluentValidation;
using ShilpoHubBD.Application.DTOs.Achievement;

namespace ShilpoHubBD.Application.Validators.Achievement;

public class AwardXpRequestValidator : AbstractValidator<AwardXpRequest>
{
    public AwardXpRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(200);
    }
}
