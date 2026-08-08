using FluentValidation;
using ShilpoHubBD.Application.DTOs.AIShopping;

namespace ShilpoHubBD.Application.Validators.AIShopping;

public class TranslationRequestValidator : AbstractValidator<TranslationRequest>
{
    public TranslationRequestValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.TargetLanguage).NotEmpty().MaximumLength(20);
    }
}
