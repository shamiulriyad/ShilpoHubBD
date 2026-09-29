using ShilpoHubBD.Application.DTOs.Certificates;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Services.Certificates;
using ShilpoHubBD.Domain.Entities.Certificate;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Certificates.Services;

[Trait("Feature", "Certificates")]
[Trait("Layer", "Service")]
public class ExpertiseCertificateServiceTests
{
    private readonly IExpertiseCertificateRepository _repository = Substitute.For<IExpertiseCertificateRepository>();
    private readonly Guid _producerId = Guid.NewGuid();

    public ExpertiseCertificateServiceTests()
    {
        _repository.GetByProducerAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(new List<ExpertiseCertificate>());
        _repository.GetProfileAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(("Jamdani weaving", true));
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private ExpertiseCertificateService CreateService() => new(_repository);

    private void SetStat(int count, double average, Guid? producerId = null)
        => _repository.GetRatingStatsAsync(Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(new List<ProducerRatingStat> { new(producerId ?? _producerId, "Rahima Begum", count, average) });

    private static ExpertiseCertificate Certificate(Guid producerId, ExpertiseLevel level, bool revoked = false) => new()
    {
        Id = Guid.NewGuid(), ProducerId = producerId, CertificateNumber = $"EXP-2026-{Random.Shared.Next(999999):D6}",
        Level = level, Expertise = "Jamdani weaving", IssuedAt = DateTime.UtcNow, IsRevoked = revoked,
        Producer = TestUsers.Create(fullName: "Rahima Begum"),
    };

    // ---------- GetMineAsync ----------

    [Fact]
    public async Task GetMineAsync_NoRatingsYet_ReportsZeroAndNoEarnedLevel()
    {
        _repository.GetRatingStatsAsync(_producerId, Arg.Any<CancellationToken>()).Returns(new List<ProducerRatingStat>());

        var dto = await CreateService().GetMineAsync(_producerId, Ct);

        Assert.Equal(0, dto.RatingCount);
        Assert.Equal(0m, dto.AverageRating);
        Assert.Null(dto.EarnedLevel);
        Assert.Equal("Bronze", dto.NextLevel);
        Assert.False(dto.AwaitingAdmin);
    }

    [Theory]
    [InlineData(4, 5.0, null)]
    [InlineData(5, 4.0, "Bronze")]
    [InlineData(9, 4.3, "Bronze")]
    [InlineData(10, 4.3, "Silver")]
    [InlineData(24, 4.6, "Silver")]
    [InlineData(25, 4.6, "Gold")]
    [InlineData(5, 3.9, null)]
    public async Task GetMineAsync_RatingCountAndAverage_DetermineTheEarnedLevel(int count, double average, string? expectedLevel)
    {
        SetStat(count, average);

        var dto = await CreateService().GetMineAsync(_producerId, Ct);

        Assert.Equal(expectedLevel, dto.EarnedLevel);
    }

    [Fact]
    public async Task GetMineAsync_AverageIsRoundedToTwoDecimalPlaces()
    {
        SetStat(5, 4.567);

        var dto = await CreateService().GetMineAsync(_producerId, Ct);

        Assert.Equal(4.57m, dto.AverageRating);
    }

    [Fact]
    public async Task GetMineAsync_EarnedHigherThanHighestIssued_AwaitingAdminIsTrue()
    {
        SetStat(10, 4.3);
        _repository.GetByProducerAsync(_producerId, Arg.Any<CancellationToken>())
            .Returns(new List<ExpertiseCertificate> { Certificate(_producerId, ExpertiseLevel.Bronze) });

        var dto = await CreateService().GetMineAsync(_producerId, Ct);

        Assert.Equal("Silver", dto.EarnedLevel);
        Assert.Equal("Bronze", dto.HighestIssuedLevel);
        Assert.True(dto.AwaitingAdmin);
    }

    [Fact]
    public async Task GetMineAsync_HighestIssuedAlreadyMatchesEarned_AwaitingAdminIsFalse()
    {
        SetStat(5, 4.0);
        _repository.GetByProducerAsync(_producerId, Arg.Any<CancellationToken>())
            .Returns(new List<ExpertiseCertificate> { Certificate(_producerId, ExpertiseLevel.Bronze) });

        var dto = await CreateService().GetMineAsync(_producerId, Ct);

        Assert.False(dto.AwaitingAdmin);
    }

    [Fact]
    public async Task GetMineAsync_RevokedCertificatesAreIgnoredWhenFindingTheHighestIssuedLevel()
    {
        SetStat(5, 4.0);
        _repository.GetByProducerAsync(_producerId, Arg.Any<CancellationToken>())
            .Returns(new List<ExpertiseCertificate> { Certificate(_producerId, ExpertiseLevel.Gold, revoked: true) });

        var dto = await CreateService().GetMineAsync(_producerId, Ct);

        Assert.Null(dto.HighestIssuedLevel);
        Assert.True(dto.AwaitingAdmin);
    }

    [Fact]
    public async Task GetMineAsync_ReturnsTheRulesInProgressionOrderFromBronzeToGold()
    {
        // Rules itself is declared highest-first (Gold, Silver, Bronze) so Earned() can check the
        // hardest level first; the DTO reverses that for display, lowest (next-to-reach) first.
        _repository.GetRatingStatsAsync(_producerId, Arg.Any<CancellationToken>()).Returns(new List<ProducerRatingStat>());

        var dto = await CreateService().GetMineAsync(_producerId, Ct);

        Assert.Equal(new[] { "Bronze", "Silver", "Gold" }, dto.Rules.Select(r => r.Level));
    }

    [Fact]
    public async Task GetMineAsync_ReportsTheProfilesApprovalStatus()
    {
        _repository.GetRatingStatsAsync(_producerId, Arg.Any<CancellationToken>()).Returns(new List<ProducerRatingStat>());
        _repository.GetProfileAsync(_producerId, Arg.Any<CancellationToken>()).Returns(((string?)null, false));

        var dto = await CreateService().GetMineAsync(_producerId, Ct);

        Assert.False(dto.ProfileApproved);
    }

    // ---------- GetEligibleAsync ----------

    [Fact]
    public async Task GetEligibleAsync_ProducerBelowTheFirstLevel_IsExcluded()
    {
        _repository.GetRatingStatsAsync(null, Arg.Any<CancellationToken>())
            .Returns(new List<ProducerRatingStat> { new(_producerId, "x", 3, 5.0) });
        _repository.GetAllActiveAsync(Arg.Any<CancellationToken>()).Returns(new List<ExpertiseCertificate>());

        Assert.Empty(await CreateService().GetEligibleAsync(Ct));
    }

    [Fact]
    public async Task GetEligibleAsync_ProducerAlreadyAtTheEarnedLevel_IsExcluded()
    {
        _repository.GetRatingStatsAsync(null, Arg.Any<CancellationToken>())
            .Returns(new List<ProducerRatingStat> { new(_producerId, "x", 5, 4.0) });
        _repository.GetAllActiveAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ExpertiseCertificate> { Certificate(_producerId, ExpertiseLevel.Bronze) });

        Assert.Empty(await CreateService().GetEligibleAsync(Ct));
    }

    [Fact]
    public async Task GetEligibleAsync_ProducerEarnedAHigherLevelThanIssued_IsIncludedWithBothLevels()
    {
        _repository.GetRatingStatsAsync(null, Arg.Any<CancellationToken>())
            .Returns(new List<ProducerRatingStat> { new(_producerId, "Rahima Begum", 10, 4.3) });
        _repository.GetAllActiveAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ExpertiseCertificate> { Certificate(_producerId, ExpertiseLevel.Bronze) });

        var result = await CreateService().GetEligibleAsync(Ct);

        var entry = Assert.Single(result);
        Assert.Equal("Silver", entry.EarnedLevel);
        Assert.Equal("Bronze", entry.HighestIssuedLevel);
        Assert.Equal("Rahima Begum", entry.ProducerName);
    }

    [Fact]
    public async Task GetEligibleAsync_NoCertificateIssuedYet_HighestIssuedLevelIsNull()
    {
        _repository.GetRatingStatsAsync(null, Arg.Any<CancellationToken>())
            .Returns(new List<ProducerRatingStat> { new(_producerId, "x", 5, 4.0) });
        _repository.GetAllActiveAsync(Arg.Any<CancellationToken>()).Returns(new List<ExpertiseCertificate>());

        var entry = Assert.Single(await CreateService().GetEligibleAsync(Ct));

        Assert.Null(entry.HighestIssuedLevel);
    }

    [Fact]
    public async Task GetEligibleAsync_OrdersByAverageRatingDescending()
    {
        var lower = Guid.NewGuid();
        var higher = Guid.NewGuid();
        _repository.GetRatingStatsAsync(null, Arg.Any<CancellationToken>()).Returns(new List<ProducerRatingStat>
        {
            new(lower, "Lower", 5, 4.0), new(higher, "Higher", 5, 4.9),
        });
        _repository.GetAllActiveAsync(Arg.Any<CancellationToken>()).Returns(new List<ExpertiseCertificate>());

        var result = await CreateService().GetEligibleAsync(Ct);

        Assert.Equal(new[] { "Higher", "Lower" }, result.Select(r => r.ProducerName));
    }

    // ---------- IssueAsync ----------

    [Fact]
    public async Task IssueAsync_EligibleProducer_IssuesTheCertificateWithASnapshotOfTheirStanding()
    {
        SetStat(10, 4.3);
        ExpertiseCertificate? saved = null;
        await _repository.AddAsync(Arg.Do<ExpertiseCertificate>(c => saved = c), Arg.Any<CancellationToken>());
        _repository.GetByProducerAsync(_producerId, Arg.Any<CancellationToken>())
            .Returns(_ => new List<ExpertiseCertificate> { saved! }.Where(c => c is not null).ToList());
        var admin = Guid.NewGuid();
        var before = DateTime.UtcNow;

        var dto = await CreateService().IssueAsync(_producerId, admin, Ct);

        Assert.NotNull(saved);
        Assert.Equal(_producerId, saved.ProducerId);
        Assert.Equal(ExpertiseLevel.Silver, saved.Level);
        Assert.Equal("Jamdani weaving", saved.Expertise);
        Assert.Equal(4.3m, saved.AverageRating);
        Assert.Equal(10, saved.RatingCount);
        Assert.Equal(admin, saved.IssuedByUserId);
        Assert.InRange(saved.IssuedAt, before, DateTime.UtcNow);
        Assert.Matches(@"^EXP-\d{4}-\d{6}$", saved.CertificateNumber);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal("Silver", dto.Level);
    }

    [Fact]
    public async Task IssueAsync_GeneratesADifferentNumberWhenTheFirstCandidateIsTaken()
    {
        SetStat(5, 4.0);
        var calls = 0;
        _repository.NumberExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(_ => calls++ == 0);
        ExpertiseCertificate? saved = null;
        await _repository.AddAsync(Arg.Do<ExpertiseCertificate>(c => saved = c), Arg.Any<CancellationToken>());
        _repository.GetByProducerAsync(_producerId, Arg.Any<CancellationToken>())
            .Returns(_ => new List<ExpertiseCertificate> { saved! }.Where(c => c is not null).ToList());

        await CreateService().IssueAsync(_producerId, Guid.NewGuid(), Ct);

        await _repository.Received(2).NumberExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IssueAsync_NoRatingsAtAll_ThrowsConflictAndSavesNothing()
    {
        _repository.GetRatingStatsAsync(_producerId, Arg.Any<CancellationToken>()).Returns(new List<ProducerRatingStat>());

        var error = await Assert.ThrowsAsync<ConflictException>(() => CreateService().IssueAsync(_producerId, Guid.NewGuid(), Ct));

        Assert.Equal("This producer has no customer ratings yet.", error.Message);
        await _repository.DidNotReceive().AddAsync(Arg.Any<ExpertiseCertificate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IssueAsync_BelowTheFirstLevel_ThrowsConflict()
    {
        SetStat(2, 5.0);

        var error = await Assert.ThrowsAsync<ConflictException>(() => CreateService().IssueAsync(_producerId, Guid.NewGuid(), Ct));

        Assert.Equal("This producer has not reached the first expertise level yet.", error.Message);
    }

    [Fact]
    public async Task IssueAsync_AlreadyHoldsTheHighestEarnedLevel_ThrowsConflictNamingTheExistingLevel()
    {
        SetStat(10, 4.3);
        _repository.GetByProducerAsync(_producerId, Arg.Any<CancellationToken>())
            .Returns(new List<ExpertiseCertificate> { Certificate(_producerId, ExpertiseLevel.Silver) });

        var error = await Assert.ThrowsAsync<ConflictException>(() => CreateService().IssueAsync(_producerId, Guid.NewGuid(), Ct));

        Assert.Equal("A Silver certificate is already issued. The producer has not earned a higher level.", error.Message);
    }

    [Fact]
    public async Task IssueAsync_ProfileNotApproved_ThrowsConflictAndSavesNothing()
    {
        SetStat(5, 4.0);
        _repository.GetProfileAsync(_producerId, Arg.Any<CancellationToken>()).Returns(("Jamdani weaving", false));

        var error = await Assert.ThrowsAsync<ConflictException>(() => CreateService().IssueAsync(_producerId, Guid.NewGuid(), Ct));

        Assert.Equal("The producer needs an approved profile with their expertise before a certificate can be issued.", error.Message);
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IssueAsync_ProfileApprovedButNoExpertiseStated_ThrowsConflict()
    {
        SetStat(5, 4.0);
        _repository.GetProfileAsync(_producerId, Arg.Any<CancellationToken>()).Returns(((string?)null, true));

        await Assert.ThrowsAsync<ConflictException>(() => CreateService().IssueAsync(_producerId, Guid.NewGuid(), Ct));
    }
}
