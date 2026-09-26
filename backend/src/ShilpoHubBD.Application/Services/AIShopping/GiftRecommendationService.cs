using ShilpoHubBD.Application.DTOs.AIShopping;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Services.AIShopping;

// Rule-based (no AI model): suggests real, approved marketplace products whose name, description or category
// matches the recipient's interest, within budget, best-rated first. Earlier this returned three fixed mock
// gifts regardless of the interest chosen.
public class GiftRecommendationService : IGiftRecommendationService
{
    private const int MaxSuggestions = 6;

    private readonly IProductRepository _productRepository;

    public GiftRecommendationService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<GiftSuggestionDto>> GetSuggestionsAsync(GiftRecommendationRequest request, CancellationToken cancellationToken)
    {
        var occasion = string.IsNullOrWhiteSpace(request.Occasion) ? "any occasion" : request.Occasion.Trim();
        var interest = request.RecipientInterest?.Trim();

        var (products, _) = await _productRepository.GetPagedAsync(new ProductQueryParameters
        {
            Search = string.IsNullOrWhiteSpace(interest) ? null : interest,
            MaxPrice = request.Budget,
            SortBy = ProductSortOption.TopRated,
            Page = 1,
            PageSize = MaxSuggestions,
        }, cancellationToken);

        return products.Select(p =>
        {
            var price = p.DiscountPrice ?? p.Price;
            var reason = string.IsNullOrWhiteSpace(interest)
                ? $"A well-rated handcrafted piece that suits {occasion}."
                : $"Matches the recipient's interest in {interest} and suits {occasion}.";
            return new GiftSuggestionDto
            {
                ProductId = p.Id,
                Slug = p.Slug,
                ImageUrl = p.Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).Select(i => i.ImageUrl).FirstOrDefault(),
                ProductName = p.Name,
                Category = p.Category?.Name ?? string.Empty,
                EstimatedPrice = price,
                Reason = request.Budget.HasValue && price <= request.Budget.Value
                    ? $"{reason} Within your budget."
                    : reason,
            };
        }).ToList();
    }
}
