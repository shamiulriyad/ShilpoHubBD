using FluentValidation;
using ShilpoHubBD.Application.DTOs.BusinessPartner;

namespace ShilpoHubBD.Application.Validators.BusinessPartner;

public class VerifyBusinessPartnerRequestValidator : AbstractValidator<VerifyBusinessPartnerRequest>
{
    public VerifyBusinessPartnerRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
