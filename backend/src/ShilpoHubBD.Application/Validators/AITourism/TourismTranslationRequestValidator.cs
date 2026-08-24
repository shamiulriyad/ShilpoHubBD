using FluentValidation;
using ShilpoHubBD.Application.DTOs.AITourism;

namespace ShilpoHubBD.Application.Validators.AITourism;

public class TourismTranslationRequestValidator : AbstractValidator<TourismTranslationRequest>
{
    public TourismTranslationRequestValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.TargetLanguage).NotEmpty().MaximumLength(50);
    }
}
