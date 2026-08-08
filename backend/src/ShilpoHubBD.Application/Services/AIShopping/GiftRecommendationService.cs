using ShilpoHubBD.Application.DTOs.AIShopping;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.AIShopping;

// Placeholder implementation returning mock data. No AI integration -- replace with a real
// AI-backed implementation later by registering a different IGiftRecommendationService.
public class GiftRecommendationService : IGiftRecommendationService
{
    public Task<List<GiftSuggestionDto>> GetSuggestionsAsync(GiftRecommendationRequest request, CancellationToken cancellationToken)
    {
        var occasion = string.IsNullOrWhiteSpace(request.Occasion) ? "any occasion" : request.Occasion;

        var suggestions = new List<GiftSuggestionDto>
        {
            new()
            {
                ProductName = "Hand-embroidered Nakshi Kantha",
                Category = "Textiles",
                EstimatedPrice = 2500,
                Reason = $"A timeless heritage piece that suits {occasion}.",
            },
            new()
            {
                ProductName = "Terracotta Decorative Vase",
                Category = "Home Decor",
                EstimatedPrice = 1200,
                Reason = $"A handcrafted keepsake fitting for {occasion}.",
            },
            new()
            {
                ProductName = "Jamdani Silk Scarf",
                Category = "Fashion",
                EstimatedPrice = 1800,
                Reason = $"An elegant accessory recommended for {occasion}.",
            },
        };

        if (request.Budget.HasValue)
        {
            suggestions = suggestions.Where(s => s.EstimatedPrice <= request.Budget.Value).ToList();
        }

        return Task.FromResult(suggestions);
    }
}
