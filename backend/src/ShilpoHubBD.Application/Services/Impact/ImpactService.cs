using ShilpoHubBD.Application.DTOs.Impact;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.Impact;

public class ImpactService : IImpactService
{
    // Simple, explainable weighting -- not AI-driven. Rewards breadth (districts, producers
    // supported) over raw volume.
    private const int PointsPerDistrict = 10;
    private const int PointsPerProducer = 5;
    private const int PointsPerItem = 1;

    // Rough estimate of CO2 saved per handmade/local item versus a mass-produced, long-shipped
    // equivalent. Not derived from per-product life-cycle data.
    private const decimal Co2SavingsPerItemKg = 2.5m;

    private readonly IImpactRepository _impactRepository;

    public ImpactService(IImpactRepository impactRepository)
    {
        _impactRepository = impactRepository;
    }

    public async Task<ImpactSummaryDto> GetMyImpactAsync(Guid userId, CancellationToken cancellationToken)
    {
        var stats = await _impactRepository.GetImpactStatsAsync(userId, cancellationToken);

        var heritageScore =
            (stats.DistinctDistrictsSupported * PointsPerDistrict) +
            (stats.FamiliesSupported * PointsPerProducer) +
            (stats.TotalItemsPurchased * PointsPerItem);

        return new ImpactSummaryDto
        {
            HeritageScore = heritageScore,
            FamiliesSupported = stats.FamiliesSupported,
            DistinctDistrictsSupported = stats.DistinctDistrictsSupported,
            DistinctCategoriesSupported = stats.DistinctCategoriesSupported,
            TotalItemsPurchased = stats.TotalItemsPurchased,
            EstimatedCo2SavingsKg = stats.TotalItemsPurchased * Co2SavingsPerItemKg,
        };
    }
}
