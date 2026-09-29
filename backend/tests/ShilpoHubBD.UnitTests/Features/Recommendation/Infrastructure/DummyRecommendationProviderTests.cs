using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Infrastructure.Recommendations;

namespace ShilpoHubBD.UnitTests.Features.Recommendation.Infrastructure;

[Trait("Feature", "Recommendation")]
[Trait("Layer", "Infrastructure")]
public class DummyRecommendationProviderTests
{
    private readonly DummyRecommendationProvider _provider = new();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Product MakeProduct(Guid categoryId, decimal rating, int sales, bool isActive = true) => new()
    {
        Id = Guid.NewGuid(), Name = "x", CategoryId = categoryId, AverageRating = rating, SalesCount = sales, IsActive = isActive,
    };

    [Fact]
    public void Name_IsDummy()
        => Assert.Equal("Dummy", _provider.Name);

    [Fact]
    public async Task RecommendForUserAsync_RanksByRatingThenSales_HighestFirst()
    {
        var categoryId = Guid.NewGuid();
        var low = MakeProduct(categoryId, 3.0m, 10);
        var high = MakeProduct(categoryId, 4.5m, 5);
        var candidates = new List<Product> { low, high };

        var result = await _provider.RecommendForUserAsync(Guid.NewGuid(), candidates, 10, Ct);

        Assert.Equal(new[] { high.Id, low.Id }, result.Select(p => p.Id));
    }

    [Fact]
    public async Task RecommendForUserAsync_SameRating_TieBreaksBySalesCount()
    {
        var categoryId = Guid.NewGuid();
        var fewerSales = MakeProduct(categoryId, 4.0m, 5);
        var moreSales = MakeProduct(categoryId, 4.0m, 50);
        var candidates = new List<Product> { fewerSales, moreSales };

        var result = await _provider.RecommendForUserAsync(Guid.NewGuid(), candidates, 10, Ct);

        Assert.Equal(new[] { moreSales.Id, fewerSales.Id }, result.Select(p => p.Id));
    }

    [Fact]
    public async Task RecommendForUserAsync_ExcludesInactiveProducts()
    {
        var categoryId = Guid.NewGuid();
        var active = MakeProduct(categoryId, 4.0m, 5);
        var inactive = MakeProduct(categoryId, 5.0m, 100, isActive: false);
        var candidates = new List<Product> { active, inactive };

        var result = await _provider.RecommendForUserAsync(Guid.NewGuid(), candidates, 10, Ct);

        Assert.Equal(active.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task RecommendForUserAsync_RespectsTheRequestedCount()
    {
        var categoryId = Guid.NewGuid();
        var candidates = Enumerable.Range(0, 5).Select(i => MakeProduct(categoryId, 4.0m, i)).ToList();

        var result = await _provider.RecommendForUserAsync(Guid.NewGuid(), candidates, 2, Ct);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task RecommendSimilarAsync_PrefersSameCategoryOverOtherCategories()
    {
        var categoryId = Guid.NewGuid();
        var otherCategoryId = Guid.NewGuid();
        var target = MakeProduct(categoryId, 5.0m, 0);
        var sameCategoryLowerRated = MakeProduct(categoryId, 2.0m, 0);
        var otherCategoryHigherRated = MakeProduct(otherCategoryId, 5.0m, 100);
        var candidates = new List<Product> { target, sameCategoryLowerRated, otherCategoryHigherRated };

        var result = await _provider.RecommendSimilarAsync(target, candidates, 10, Ct);

        Assert.Equal(new[] { sameCategoryLowerRated.Id, otherCategoryHigherRated.Id }, result.Select(p => p.Id));
    }

    [Fact]
    public async Task RecommendSimilarAsync_ExcludesTheProductItself()
    {
        var categoryId = Guid.NewGuid();
        var target = MakeProduct(categoryId, 5.0m, 0);
        var candidates = new List<Product> { target };

        var result = await _provider.RecommendSimilarAsync(target, candidates, 10, Ct);

        Assert.Empty(result);
    }
}
