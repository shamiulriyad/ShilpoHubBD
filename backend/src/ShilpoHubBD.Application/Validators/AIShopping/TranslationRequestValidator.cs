using FluentValidation;
using ShilpoHubBD.Application.DTOs.AIShopping;

namespace ShilpoHubBD.Application.Validators.AIShopping;

public class TranslationRequestValidator : AbstractValidator<TranslationRequest>
{
    // Accepted language codes/names, case-insensitive. Bangla and English are the guaranteed floor;
    // the rest are common languages a Bangladeshi marketplace's buyers/producers realistically use.
    // Kept as an explicit allow-list (rather than accepting any string) so a typo or garbage value is
    // rejected up front as a validation error instead of silently reaching the AI provider.
    public static readonly HashSet<string> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "en", "english",
        "bn", "bangla", "bengali",
        "hi", "hindi",
        "ur", "urdu",
        "ar", "arabic",
        "zh", "chinese", "mandarin",
        "fr", "french",
        "es", "spanish",
        "de", "german",
        "ja", "japanese",
    };

    public TranslationRequestValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.TargetLanguage)
            .NotEmpty().MaximumLength(20)
            .Must(lang => SupportedLanguages.Contains(lang.Trim()))
            .WithMessage(
                $"Unsupported target language. Supported languages: {string.Join(", ", new[] { "English", "Bangla", "Hindi", "Urdu", "Arabic", "Chinese", "French", "Spanish", "German", "Japanese" })}.")
            .When(x => !string.IsNullOrWhiteSpace(x.TargetLanguage));
    }
}
