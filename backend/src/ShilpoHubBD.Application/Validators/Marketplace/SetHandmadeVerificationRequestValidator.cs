using FluentValidation;
using ShilpoHubBD.Application.DTOs.Marketplace;

namespace ShilpoHubBD.Application.Validators.Marketplace;

public class SetHandmadeVerificationRequestValidator : AbstractValidator<SetHandmadeVerificationRequest>
{
    public SetHandmadeVerificationRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
