using FluentValidation;
using ShilpoHubBD.Application.DTOs.AIBusiness;

namespace ShilpoHubBD.Application.Validators.AIBusiness;

public class BusinessTranslationRequestValidator : AbstractValidator<BusinessTranslationRequest>
{
    public BusinessTranslationRequestValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.TargetLanguage).NotEmpty().MaximumLength(50);
    }
}
