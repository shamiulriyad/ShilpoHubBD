using ShilpoHubBD.Application.DTOs.SupplierMatching;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.SupplierMatching;

// Rule-based (not AI) matching engine: scores each producer against the criteria the caller
// actually specified, so an unspecified filter neither helps nor hurts a candidate's score.
// Overall rating is always factored in as a small baseline quality signal.
public class SupplierMatchingService : ISupplierMatchingService
{
    private const decimal RatingWeight = 10m;
    private const decimal CategoryWeight = 15m;
    private const decimal KeywordWeight = 15m;
    private const decimal QuantityWeight = 15m;
    private const decimal BudgetWeight = 15m;
    private const decimal LocationWeight = 10m;
    private const decimal MaterialWeight = 10m;
    private const decimal CertificationWeight = 10m;
    private const decimal DeliveryWeight = 10m;

    private readonly ISupplierMatchingRepository _supplierMatchingRepository;

    public SupplierMatchingService(ISupplierMatchingRepository supplierMatchingRepository)
    {
        _supplierMatchingRepository = supplierMatchingRepository;
    }

    public async Task<List<SupplierMatchResultDto>> MatchAsync(SupplierMatchRequest request, CancellationToken cancellationToken)
    {
        var candidates = await _supplierMatchingRepository.GetCandidatesAsync(request, cancellationToken);

        return candidates
            .Select(c => Score(c, request))
            .OrderByDescending(r => r.MatchScore)
            .ThenByDescending(r => r.AverageRating)
            .Take(request.MaxResults)
            .ToList();
    }

    private static SupplierMatchResultDto Score(SupplierMatchCandidateDto candidate, SupplierMatchRequest request)
    {
        var totalWeight = 0m;
        var achievedWeight = 0m;
        var reasons = new List<string>();

        totalWeight += RatingWeight;
        var ratingFraction = Math.Min(1m, candidate.AverageRating / 5m);
        achievedWeight += RatingWeight * ratingFraction;
        if (candidate.TotalReviewCount > 0)
        {
            reasons.Add($"Rated {candidate.AverageRating:0.0}/5 from {candidate.TotalReviewCount} review(s).");
        }

        if (request.MinRating.HasValue && candidate.AverageRating >= request.MinRating.Value)
        {
            reasons.Add($"Meets the minimum rating of {request.MinRating.Value:0.0}.");
        }

        if (request.CategoryId.HasValue)
        {
            totalWeight += CategoryWeight;
            if (candidate.HasMatchingCategory)
            {
                achievedWeight += CategoryWeight;
                reasons.Add("Offers products in the requested category.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.ProductKeyword))
        {
            totalWeight += KeywordWeight;
            if (candidate.HasMatchingKeyword)
            {
                achievedWeight += KeywordWeight;
                reasons.Add($"Has products matching \"{request.ProductKeyword.Trim()}\".");
            }
        }

        if (request.Quantity.HasValue && request.Quantity.Value > 0)
        {
            totalWeight += QuantityWeight;
            var fraction = Math.Min(1m, (decimal)candidate.EstimatedProductionCapacity / request.Quantity.Value);
            achievedWeight += QuantityWeight * fraction;

            if (fraction >= 1m)
            {
                reasons.Add($"Estimated capacity ({candidate.EstimatedProductionCapacity} units) covers the requested quantity ({request.Quantity.Value}).");
            }
            else if (fraction > 0)
            {
                reasons.Add($"Partial capacity match: ~{candidate.EstimatedProductionCapacity} of {request.Quantity.Value} units requested.");
            }
        }

        if (request.MaxBudgetPerUnit.HasValue)
        {
            totalWeight += BudgetWeight;
            if (candidate.HasProductWithinBudget)
            {
                achievedWeight += BudgetWeight;
                reasons.Add($"Has products at or under the budget of {request.MaxBudgetPerUnit.Value:0.##}.");
            }
        }

        if (request.DistrictId.HasValue)
        {
            totalWeight += LocationWeight;
            if (candidate.HasMatchingDistrict)
            {
                achievedWeight += LocationWeight;
                reasons.Add("Located in the requested district.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Material))
        {
            totalWeight += MaterialWeight;
            if (candidate.HasMatchingMaterial)
            {
                achievedWeight += MaterialWeight;
                reasons.Add($"Works with the requested material \"{request.Material.Trim()}\".");
            }
        }

        if (request.CertificationRequired == true)
        {
            totalWeight += CertificationWeight;
            if (candidate.CertificationCount > 0)
            {
                achievedWeight += CertificationWeight;
                reasons.Add($"Holds {candidate.CertificationCount} certification(s).");
            }
        }

        if (request.MaxDeliveryDays.HasValue)
        {
            totalWeight += DeliveryWeight;
            if (candidate.AverageDeliveryDays.HasValue)
            {
                if (candidate.AverageDeliveryDays.Value <= request.MaxDeliveryDays.Value)
                {
                    achievedWeight += DeliveryWeight;
                    reasons.Add($"Average delivery time ({candidate.AverageDeliveryDays.Value:0.#} days) meets the {request.MaxDeliveryDays.Value}-day requirement.");
                }
                else
                {
                    // Has a track record, just slower than requested -- partial credit rather than zero.
                    achievedWeight += DeliveryWeight * 0.25m;
                }
            }
            else
            {
                // No delivery history yet -- neutral rather than penalized.
                achievedWeight += DeliveryWeight * 0.5m;
            }
        }

        var score = totalWeight == 0m ? 0m : Math.Round(achievedWeight / totalWeight * 100m, 1);

        return new SupplierMatchResultDto
        {
            ProducerId = candidate.ProducerId,
            ProducerName = candidate.ProducerName,
            WorkshopName = candidate.WorkshopName,
            PrimaryCraft = candidate.PrimaryCraft,
            DistrictName = candidate.DistrictName,
            MatchScore = score,
            MatchReasons = reasons,
            AverageRating = candidate.AverageRating,
            ProductCount = candidate.ProductCount,
            MinPrice = candidate.MinPrice,
            EstimatedProductionCapacity = candidate.EstimatedProductionCapacity,
            CertificationCount = candidate.CertificationCount,
            IsHandmadeVerified = candidate.IsHandmadeVerified,
            AverageDeliveryDays = candidate.AverageDeliveryDays,
        };
    }
}
