using ShilpoHubBD.Application.DTOs.SupplierMatching;
using ShilpoHubBD.Application.Interfaces.Repositories;
using SupplierMatchingService = ShilpoHubBD.Application.Services.SupplierMatching.SupplierMatchingService;

namespace ShilpoHubBD.UnitTests.Features.SupplierMatching.Services;

[Trait("Feature", "SupplierMatching")]
[Trait("Layer", "Service")]
public class SupplierMatchingServiceTests
{
    private readonly ISupplierMatchingRepository _repository = Substitute.For<ISupplierMatchingRepository>();
    private readonly SupplierMatchingService _service;

    public SupplierMatchingServiceTests()
    {
        _service = new SupplierMatchingService(_repository);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static SupplierMatchCandidateDto MakeCandidate(decimal rating = 0, int reviewCount = 0) => new()
    {
        ProducerId = Guid.NewGuid(), ProducerName = "Producer", AverageRating = rating, TotalReviewCount = reviewCount,
    };

    [Fact]
    public async Task MatchAsync_NoFiltersSpecified_ScoresPurelyOnRating()
    {
        var candidate = MakeCandidate(rating: 5m);
        _repository.GetCandidatesAsync(Arg.Any<SupplierMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<SupplierMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new SupplierMatchRequest(), Ct);

        Assert.Equal(100m, Assert.Single(result).MatchScore);
    }

    [Fact]
    public async Task MatchAsync_ZeroRatingAndNoFilters_ScoresZero()
    {
        var candidate = MakeCandidate(rating: 0m);
        _repository.GetCandidatesAsync(Arg.Any<SupplierMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<SupplierMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new SupplierMatchRequest(), Ct);

        Assert.Equal(0m, Assert.Single(result).MatchScore);
    }

    [Fact]
    public async Task MatchAsync_HasReviews_AddsARatingReason()
    {
        var candidate = MakeCandidate(rating: 4.5m, reviewCount: 3);
        _repository.GetCandidatesAsync(Arg.Any<SupplierMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<SupplierMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new SupplierMatchRequest(), Ct);

        Assert.Contains("Rated 4.5/5 from 3 review(s).", Assert.Single(result).MatchReasons);
    }

    [Fact]
    public async Task MatchAsync_MatchingCategory_AddsToScoreAndReason()
    {
        var candidate = MakeCandidate();
        candidate.HasMatchingCategory = true;
        _repository.GetCandidatesAsync(Arg.Any<SupplierMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<SupplierMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new SupplierMatchRequest { CategoryId = Guid.NewGuid() }, Ct);

        Assert.Contains("Offers products in the requested category.", Assert.Single(result).MatchReasons);
    }

    [Fact]
    public async Task MatchAsync_MatchingKeyword_AddsReasonWithTrimmedKeyword()
    {
        var candidate = MakeCandidate();
        candidate.HasMatchingKeyword = true;
        _repository.GetCandidatesAsync(Arg.Any<SupplierMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<SupplierMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new SupplierMatchRequest { ProductKeyword = "  kantha  " }, Ct);

        Assert.Contains("Has products matching \"kantha\".", Assert.Single(result).MatchReasons);
    }

    [Fact]
    public async Task MatchAsync_CapacityFullyCoversQuantity_AddsTheFullCoverageReason()
    {
        var candidate = MakeCandidate();
        candidate.EstimatedProductionCapacity = 200;
        _repository.GetCandidatesAsync(Arg.Any<SupplierMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<SupplierMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new SupplierMatchRequest { Quantity = 100 }, Ct);

        Assert.Contains(Assert.Single(result).MatchReasons, r => r.Contains("covers the requested quantity"));
    }

    [Fact]
    public async Task MatchAsync_CapacityPartiallyCoversQuantity_AddsThePartialMatchReasonAndPartialCredit()
    {
        var candidate = MakeCandidate();
        candidate.EstimatedProductionCapacity = 50;
        _repository.GetCandidatesAsync(Arg.Any<SupplierMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<SupplierMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new SupplierMatchRequest { Quantity = 100 }, Ct);

        var dto = Assert.Single(result);
        Assert.Contains(dto.MatchReasons, r => r.Contains("Partial capacity match"));
        Assert.True(dto.MatchScore > 0m && dto.MatchScore < 100m);
    }

    [Fact]
    public async Task MatchAsync_ZeroCapacity_AddsNoCapacityReason()
    {
        var candidate = MakeCandidate();
        candidate.EstimatedProductionCapacity = 0;
        _repository.GetCandidatesAsync(Arg.Any<SupplierMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<SupplierMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new SupplierMatchRequest { Quantity = 100 }, Ct);

        Assert.DoesNotContain(Assert.Single(result).MatchReasons, r => r.Contains("capacity"));
    }

    [Fact]
    public async Task MatchAsync_HasProductWithinBudget_AddsReason()
    {
        var candidate = MakeCandidate();
        candidate.HasProductWithinBudget = true;
        _repository.GetCandidatesAsync(Arg.Any<SupplierMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<SupplierMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new SupplierMatchRequest { MaxBudgetPerUnit = 500 }, Ct);

        Assert.Contains(Assert.Single(result).MatchReasons, r => r.Contains("budget"));
    }

    [Fact]
    public async Task MatchAsync_MatchingDistrict_AddsReason()
    {
        var candidate = MakeCandidate();
        candidate.HasMatchingDistrict = true;
        _repository.GetCandidatesAsync(Arg.Any<SupplierMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<SupplierMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new SupplierMatchRequest { DistrictId = Guid.NewGuid() }, Ct);

        Assert.Contains("Located in the requested district.", Assert.Single(result).MatchReasons);
    }

    [Fact]
    public async Task MatchAsync_MatchingMaterial_AddsReasonWithTrimmedMaterial()
    {
        var candidate = MakeCandidate();
        candidate.HasMatchingMaterial = true;
        _repository.GetCandidatesAsync(Arg.Any<SupplierMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<SupplierMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new SupplierMatchRequest { Material = "  Cotton  " }, Ct);

        Assert.Contains("Works with the requested material \"Cotton\".", Assert.Single(result).MatchReasons);
    }

    [Fact]
    public async Task MatchAsync_CertificationRequiredAndHeld_AddsReason()
    {
        var candidate = MakeCandidate();
        candidate.CertificationCount = 2;
        _repository.GetCandidatesAsync(Arg.Any<SupplierMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<SupplierMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new SupplierMatchRequest { CertificationRequired = true }, Ct);

        Assert.Contains("Holds 2 certification(s).", Assert.Single(result).MatchReasons);
    }

    [Fact]
    public async Task MatchAsync_CertificationRequiredButNoneHeld_ScoresZeroForThatCriterion()
    {
        var candidate = MakeCandidate();
        candidate.CertificationCount = 0;
        _repository.GetCandidatesAsync(Arg.Any<SupplierMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<SupplierMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new SupplierMatchRequest { CertificationRequired = true }, Ct);

        Assert.Equal(0m, Assert.Single(result).MatchScore);
    }

    [Fact]
    public async Task MatchAsync_DeliveryWithinRequestedDays_AddsFullReasonAndCredit()
    {
        var candidate = MakeCandidate();
        candidate.AverageDeliveryDays = 3;
        _repository.GetCandidatesAsync(Arg.Any<SupplierMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<SupplierMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new SupplierMatchRequest { MaxDeliveryDays = 5 }, Ct);

        // Rating weight (10) still counts toward the denominator even at 0 rating, diluting the fully
        // achieved delivery weight (10): 10 / (10 + 10) * 100 = 50.
        var dto = Assert.Single(result);
        Assert.Contains(dto.MatchReasons, r => r.Contains("meets the 5-day requirement"));
        Assert.Equal(50m, dto.MatchScore);
    }

    [Fact]
    public async Task MatchAsync_DeliverySlowerThanRequested_GetsPartialCreditWithoutAReason()
    {
        var candidate = MakeCandidate();
        candidate.AverageDeliveryDays = 10;
        _repository.GetCandidatesAsync(Arg.Any<SupplierMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<SupplierMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new SupplierMatchRequest { MaxDeliveryDays = 5 }, Ct);

        // Delivery gets 25% credit (2.5 of 10) for having a track record but missing the target;
        // rating (0 of 10) stays unachieved: 2.5 / (10 + 10) * 100 = 12.5.
        var dto = Assert.Single(result);
        Assert.Equal(12.5m, dto.MatchScore);
        Assert.DoesNotContain(dto.MatchReasons, r => r.Contains("day requirement"));
    }

    [Fact]
    public async Task MatchAsync_NoDeliveryHistory_GetsNeutralHalfCredit()
    {
        var candidate = MakeCandidate();
        candidate.AverageDeliveryDays = null;
        _repository.GetCandidatesAsync(Arg.Any<SupplierMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<SupplierMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new SupplierMatchRequest { MaxDeliveryDays = 5 }, Ct);

        // No delivery history gets a neutral 50% credit (5 of 10); rating (0 of 10) stays unachieved:
        // 5 / (10 + 10) * 100 = 25.
        Assert.Equal(25m, Assert.Single(result).MatchScore);
    }

    [Fact]
    public async Task MatchAsync_OrdersByScoreThenAverageRating_AndRespectsMaxResults()
    {
        var lowScore = MakeCandidate(rating: 1m);
        var highScoreLowerRated = MakeCandidate(rating: 3m);
        highScoreLowerRated.HasMatchingCategory = true;
        var highScoreHigherRated = MakeCandidate(rating: 5m);
        highScoreHigherRated.HasMatchingCategory = true;

        _repository.GetCandidatesAsync(Arg.Any<SupplierMatchRequest>(), Arg.Any<CancellationToken>())
            .Returns(new List<SupplierMatchCandidateDto> { lowScore, highScoreLowerRated, highScoreHigherRated });

        var result = await _service.MatchAsync(new SupplierMatchRequest { CategoryId = Guid.NewGuid(), MaxResults = 2 }, Ct);

        Assert.Equal(2, result.Count);
        Assert.Equal(highScoreHigherRated.ProducerId, result[0].ProducerId);
        Assert.Equal(highScoreLowerRated.ProducerId, result[1].ProducerId);
    }
}
