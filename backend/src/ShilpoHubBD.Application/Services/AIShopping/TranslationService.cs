using ShilpoHubBD.Application.DTOs.AIShopping;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.AIShopping;

// Placeholder implementation returning mock data. No AI integration -- replace with a real
// AI-backed implementation later by registering a different ITranslationService.
public class TranslationService : ITranslationService
{
    public Task<TranslationResultDto> TranslateAsync(TranslationRequest request, CancellationToken cancellationToken)
    {
        var result = new TranslationResultDto
        {
            OriginalText = request.Text,
            TranslatedText = $"[{request.TargetLanguage}] {request.Text}",
            TargetLanguage = request.TargetLanguage,
        };

        return Task.FromResult(result);
    }
}
