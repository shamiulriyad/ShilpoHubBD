using ShilpoHubBD.Application.DTOs.AIShopping;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ITranslationService
{
    Task<TranslationResultDto> TranslateAsync(TranslationRequest request, CancellationToken cancellationToken);
}
