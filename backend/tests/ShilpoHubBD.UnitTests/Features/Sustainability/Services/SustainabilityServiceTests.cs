using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.Sustainability;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Options;
using ShilpoHubBD.Domain.Entities.Sustainability;
using SustainabilityService = ShilpoHubBD.Application.Services.Sustainability.SustainabilityService;

namespace ShilpoHubBD.UnitTests.Features.Sustainability.Services;

[Trait("Feature", "Sustainability")]
[Trait("Layer", "Service")]
public class SustainabilityServiceTests
{
    private readonly ISustainabilityRepository _repository = Substitute.For<ISustainabilityRepository>();
    private readonly SustainabilityService _service;

    public SustainabilityServiceTests()
    {
        _service = new SustainabilityService(_repository, Options.Create(new SustainabilityScoreOptions()));
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static SustainabilityProfile MakeProfile(Guid producerId) => new()
    {
        Id = Guid.NewGuid(), ProducerId = producerId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
    };

    [Fact]
    public async Task GetMyProfileAsync_NoExistingProfile_CreatesOneWithZeroScoreAndNoBadge()
    {
        var producerId = Guid.NewGuid();
        _repository.GetByProducerIdAsync(producerId, Arg.Any<CancellationToken>()).Returns((SustainabilityProfile?)null);

        var result = await _service.GetMyProfileAsync(producerId, Ct);

        await _repository.Received(1).AddAsync(Arg.Is<SustainabilityProfile>(p => p.ProducerId == producerId && p.EcoScore == 0), Ct);
        await _repository.Received(1).SaveChangesAsync(Ct);
        Assert.Equal(0, result.EcoScore);
        Assert.Equal("None", result.BadgeLevel);
    }

    [Fact]
    public async Task GetMyProfileAsync_ExistingProfile_ReturnsItWithoutCreatingANewOne()
    {
        var producerId = Guid.NewGuid();
        var profile = MakeProfile(producerId);
        _repository.GetByProducerIdAsync(producerId, Arg.Any<CancellationToken>()).Returns(profile);

        var result = await _service.GetMyProfileAsync(producerId, Ct);

        await _repository.DidNotReceive().AddAsync(Arg.Any<SustainabilityProfile>(), Ct);
        Assert.Equal(profile.Id, result.Id);
    }

    [Fact]
    public async Task GetByProducerIdAsync_UnknownProducer_ThrowsNotFound()
    {
        _repository.GetByProducerIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((SustainabilityProfile?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByProducerIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetByProducerIdAsync_Known_ReturnsIt()
    {
        var producerId = Guid.NewGuid();
        var profile = MakeProfile(producerId);
        _repository.GetByProducerIdAsync(producerId, Arg.Any<CancellationToken>()).Returns(profile);

        var result = await _service.GetByProducerIdAsync(producerId, Ct);

        Assert.Equal(profile.Id, result.Id);
    }

    [Fact]
    public async Task AddMaterialRecordAsync_NoExistingProfile_CreatesOneFirst()
    {
        var producerId = Guid.NewGuid();
        _repository.GetByProducerIdAsync(producerId, Arg.Any<CancellationToken>()).Returns((SustainabilityProfile?)null);

        var request = new CreateMaterialRecordRequest { MaterialName = "  Cotton  ", QuantityUsed = 2, Unit = "  kg  ", CarbonSavingsPerUnitKg = 1 };
        var result = await _service.AddMaterialRecordAsync(producerId, request, Ct);

        await _repository.Received(1).AddAsync(Arg.Any<SustainabilityProfile>(), Ct);
        await _repository.Received(1).AddMaterialRecordAsync(Arg.Is<SustainableMaterialRecord>(r => r.MaterialName == "Cotton" && r.Unit == "kg"), Ct);
        Assert.Equal("Cotton", result.MaterialName);
    }

    [Fact]
    public async Task AddMaterialRecordAsync_ComputesTotalCarbonSavingsAsQuantityTimesPerUnit()
    {
        var producerId = Guid.NewGuid();
        _repository.GetByProducerIdAsync(producerId, Arg.Any<CancellationToken>()).Returns(MakeProfile(producerId));

        var request = new CreateMaterialRecordRequest { MaterialName = "Cotton", QuantityUsed = 3, Unit = "kg", CarbonSavingsPerUnitKg = 2.5m };
        var result = await _service.AddMaterialRecordAsync(producerId, request, Ct);

        Assert.Equal(7.5m, result.TotalCarbonSavingsKg);
    }

    [Fact]
    public async Task AddMaterialRecordAsync_RecycledRenewableLocalAndBiodegradable_EarnsPointsForEachFlag()
    {
        var producerId = Guid.NewGuid();
        _repository.GetByProducerIdAsync(producerId, Arg.Any<CancellationToken>()).Returns(MakeProfile(producerId));

        var request = new CreateMaterialRecordRequest
        {
            MaterialName = "Cotton", QuantityUsed = 1, Unit = "kg",
            IsRecycled = true, IsRenewable = true, IsLocallySourced = true, IsBiodegradable = true,
        };
        await _service.AddMaterialRecordAsync(producerId, request, Ct);

        // Default weights: 5 + 5 + 3 + 4 = 17
        await _repository.Received(1).SaveChangesAsync(Ct);
        var profileSnapshot = await _repository.GetByProducerIdAsync(producerId, Ct);
        Assert.Equal(17, profileSnapshot!.EcoScore);
    }

    [Theory]
    [InlineData(0, "None")]
    [InlineData(3, "Bronze")]
    [InlineData(6, "Silver")]
    [InlineData(9, "Gold")]
    public async Task AddCertificationAsync_VerifiedCertificationCount_MapsEcoScoreToTheRightBadge(int verifiedCertificationCount, string expectedBadge)
    {
        // Each verified certification is worth 10 points (default PointsPerVerifiedCertification), so
        // verifiedCertificationCount * 10 gives the eco score: 0, 30, 60, 90 -> None, Bronze, Silver, Gold.
        var producerId = Guid.NewGuid();
        var profile = MakeProfile(producerId);
        for (var i = 0; i < verifiedCertificationCount; i++)
        {
            profile.Certifications.Add(new SustainableMaterialCertification { Id = Guid.NewGuid(), IsVerified = true });
        }

        _repository.GetByProducerIdAsync(producerId, Arg.Any<CancellationToken>()).Returns(profile);

        var request = new CreateMaterialCertificationRequest { MaterialName = "x", CertifyingBody = "x", CertificateReference = "x", IssuedAt = DateTime.UtcNow };
        await _service.AddCertificationAsync(producerId, request, Ct);

        Assert.Equal(expectedBadge, profile.BadgeLevel.ToString());
    }

    [Fact]
    public async Task AddCertificationAsync_TrimsFieldsAndStartsUnverified()
    {
        var producerId = Guid.NewGuid();
        _repository.GetByProducerIdAsync(producerId, Arg.Any<CancellationToken>()).Returns(MakeProfile(producerId));

        var request = new CreateMaterialCertificationRequest
        {
            MaterialName = "  Cotton  ", CertifyingBody = "  GOTS  ", CertificateReference = "  GOTS-123  ", IssuedAt = DateTime.UtcNow,
        };
        var result = await _service.AddCertificationAsync(producerId, request, Ct);

        Assert.Equal("Cotton", result.MaterialName);
        Assert.Equal("GOTS", result.CertifyingBody);
        Assert.Equal("GOTS-123", result.CertificateReference);
        Assert.False(result.IsVerified);
    }

    [Fact]
    public async Task VerifyCertificationAsync_UnknownCertification_ThrowsNotFound()
    {
        _repository.GetCertificationByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((SustainableMaterialCertification?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.VerifyCertificationAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task VerifyCertificationAsync_AlreadyVerified_ThrowsConflict()
    {
        var certification = new SustainableMaterialCertification { Id = Guid.NewGuid(), IsVerified = true, SustainabilityProfileId = Guid.NewGuid() };
        _repository.GetCertificationByIdAsync(certification.Id, Arg.Any<CancellationToken>()).Returns(certification);

        await Assert.ThrowsAsync<ConflictException>(() => _service.VerifyCertificationAsync(certification.Id, Ct));
    }

    [Fact]
    public async Task VerifyCertificationAsync_Valid_SetsVerifiedAndRecalculatesTheProfile()
    {
        var profile = MakeProfile(Guid.NewGuid());
        var certification = new SustainableMaterialCertification { Id = Guid.NewGuid(), IsVerified = false, SustainabilityProfileId = profile.Id };
        profile.Certifications.Add(certification);
        _repository.GetCertificationByIdAsync(certification.Id, Arg.Any<CancellationToken>()).Returns(certification);
        _repository.GetByIdAsync(profile.Id, Arg.Any<CancellationToken>()).Returns(profile);

        var result = await _service.VerifyCertificationAsync(certification.Id, Ct);

        Assert.True(result.IsVerified);
        Assert.Equal(10, profile.EcoScore);
        await _repository.Received(1).SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task VerifyCertificationAsync_ProfileNoLongerFound_StillVerifiesWithoutThrowing()
    {
        var certification = new SustainableMaterialCertification { Id = Guid.NewGuid(), IsVerified = false, SustainabilityProfileId = Guid.NewGuid() };
        _repository.GetCertificationByIdAsync(certification.Id, Arg.Any<CancellationToken>()).Returns(certification);
        _repository.GetByIdAsync(certification.SustainabilityProfileId, Arg.Any<CancellationToken>()).Returns((SustainabilityProfile?)null);

        var result = await _service.VerifyCertificationAsync(certification.Id, Ct);

        Assert.True(result.IsVerified);
    }
}
