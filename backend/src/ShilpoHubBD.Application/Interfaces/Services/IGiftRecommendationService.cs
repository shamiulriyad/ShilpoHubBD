using ShilpoHubBD.Application.DTOs.AIShopping;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IGiftRecommendationService
{
    Task<List<GiftSuggestionDto>> GetSuggestionsAsync(GiftRecommendationRequest request, CancellationToken cancellationToken);
}
