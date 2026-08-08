using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Infrastructure.Recommendations;

// Rule-based stand-in for a future AI/ML recommendation engine: ranks purely by rating and sales
// volume. userId is accepted (for a future personalized implementation) but unused here.
public class DummyRecommendationProvider : IRecommendationProvider
{
    public string Name { get; } = "Dummy";

    public Task<List<Product>> RecommendForUserAsync(Guid? userId, List<Product> candidates, int count, CancellationToken cancellationToken)
    {
        var result = RankByPopularity(candidates).Take(count).ToList();
        return Task.FromResult(result);
    }

    public Task<List<Product>> RecommendSimilarAsync(Product product, List<Product> candidates, int count, CancellationToken cancellationToken)
    {
        var others = candidates.Where(p => p.Id != product.Id);

        var sameCategory = RankByPopularity(others.Where(p => p.CategoryId == product.CategoryId));
        var otherCategories = RankByPopularity(others.Where(p => p.CategoryId != product.CategoryId));

        var result = sameCategory.Concat(otherCategories).Take(count).ToList();
        return Task.FromResult(result);
    }

    private static IEnumerable<Product> RankByPopularity(IEnumerable<Product> products)
        => products
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.AverageRating)
            .ThenByDescending(p => p.SalesCount);
}
