using ShilpoHubBD.Application.DTOs.Impact;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ImpactService = ShilpoHubBD.Application.Services.Impact.ImpactService;

namespace ShilpoHubBD.UnitTests.Features.Impact.Services;

[Trait("Feature", "Impact")]
[Trait("Layer", "Service")]
public class ImpactServiceTests
{
    private readonly IImpactRepository _repository = Substitute.For<IImpactRepository>();
    private readonly ImpactService _service;

    public ImpactServiceTests()
    {
        _service = new ImpactService(_repository);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetMyImpactAsync_NoActivity_ReturnsAllZeroes()
    {
        var userId = Guid.NewGuid();
        _repository.GetImpactStatsAsync(userId, Arg.Any<CancellationToken>()).Returns(new ImpactStatsDto());

        var result = await _service.GetMyImpactAsync(userId, Ct);

        Assert.Equal(0, result.HeritageScore);
        Assert.Equal(0, result.EstimatedCo2SavingsKg);
    }

    [Fact]
    public async Task GetMyImpactAsync_CombinesStatsIntoAWeightedHeritageScore()
    {
        var userId = Guid.NewGuid();
        _repository.GetImpactStatsAsync(userId, Arg.Any<CancellationToken>()).Returns(new ImpactStatsDto
        {
            FamiliesSupported = 3,
            DistinctDistrictsSupported = 2,
            DistinctCategoriesSupported = 4,
            TotalItemsPurchased = 10,
        });

        var result = await _service.GetMyImpactAsync(userId, Ct);

        // 2 districts * 10 + 3 families * 5 + 10 items * 1 = 20 + 15 + 10 = 45
        Assert.Equal(45, result.HeritageScore);
        Assert.Equal(3, result.FamiliesSupported);
        Assert.Equal(2, result.DistinctDistrictsSupported);
        Assert.Equal(4, result.DistinctCategoriesSupported);
        Assert.Equal(10, result.TotalItemsPurchased);
    }

    [Fact]
    public async Task GetMyImpactAsync_EstimatesCo2SavingsFromItemsPurchased()
    {
        var userId = Guid.NewGuid();
        _repository.GetImpactStatsAsync(userId, Arg.Any<CancellationToken>()).Returns(new ImpactStatsDto { TotalItemsPurchased = 4 });

        var result = await _service.GetMyImpactAsync(userId, Ct);

        Assert.Equal(10.0m, result.EstimatedCo2SavingsKg);
    }
}
