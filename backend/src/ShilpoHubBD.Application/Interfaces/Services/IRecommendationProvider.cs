using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Interfaces.Services;

// Abstraction over how recommendations are ranked. Swap in a real ML-backed implementation later
// and register it in place of DummyRecommendationProvider -- no changes needed in RecommendationService.
public interface IRecommendationProvider
{
    string Name { get; }

    Task<List<Product>> RecommendForUserAsync(Guid? userId, List<Product> candidates, int count, CancellationToken cancellationToken);
    Task<List<Product>> RecommendSimilarAsync(Product product, List<Product> candidates, int count, CancellationToken cancellationToken);
}
