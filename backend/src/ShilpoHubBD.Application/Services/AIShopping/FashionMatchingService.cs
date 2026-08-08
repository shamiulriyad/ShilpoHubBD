using ShilpoHubBD.Application.DTOs.AIShopping;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.AIShopping;

// Placeholder implementation returning mock data. No AI integration -- replace with a real
// AI-backed implementation later by registering a different IFashionMatchingService.
public class FashionMatchingService : IFashionMatchingService
{
    public Task<List<FashionMatchDto>> GetMatchesAsync(FashionMatchRequest request, CancellationToken cancellationToken)
    {
        var item = string.IsNullOrWhiteSpace(request.ItemDescription) ? "your item" : request.ItemDescription;

        var matches = new List<FashionMatchDto>
        {
            new()
            {
                ItemName = "Handwoven Cotton Shawl",
                MatchType = "Accessory",
                Reason = $"Complements the tones and texture of {item}.",
            },
            new()
            {
                ItemName = "Nakshi Katha Clutch Bag",
                MatchType = "Bag",
                Reason = $"A traditional pairing that matches {item}.",
            },
            new()
            {
                ItemName = "Brass Filigree Earrings",
                MatchType = "Jewelry",
                Reason = $"Adds a finishing touch to an outfit built around {item}.",
            },
        };

        return Task.FromResult(matches);
    }
}
