using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageIdentity;

namespace ShilpoHubBD.Application.Validators.HeritageIdentity;

public class VerifyHeritageIdentityRequestValidator : AbstractValidator<VerifyHeritageIdentityRequest>
{
    public VerifyHeritageIdentityRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
