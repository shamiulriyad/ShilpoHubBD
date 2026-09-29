using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Entities.Sustainability;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Sustainability.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Sustainability")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class SustainabilityRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public SustainabilityRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<SustainabilityProfile> SeedProfileAsync(ShilpoHubDbContext context)
    {
        var producer = TestUsers.Create();
        var profile = new SustainabilityProfile
        {
            Id = Guid.NewGuid(), ProducerId = producer.Id, EcoScore = 0, BadgeLevel = GreenBadgeLevel.None,
            TotalCarbonSavingsKg = 0, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        context.Users.Add(producer);
        context.SustainabilityProfiles.Add(profile);
        await context.SaveChangesAsync(Ct);
        return profile;
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var scope = await _database.BeginAsync();
        var repository = new SustainabilityRepository(scope.NewContext());

        Assert.Null(await repository.GetByIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetByIdAsync_KnownId_ReturnsItWithMaterialRecordsAndCertificationsLoaded()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var profile = await SeedProfileAsync(seedContext);
        seedContext.SustainableMaterialRecords.Add(new SustainableMaterialRecord
        {
            Id = Guid.NewGuid(), SustainabilityProfileId = profile.Id, MaterialName = "Cotton", QuantityUsed = 1, Unit = "kg", RecordedAt = DateTime.UtcNow,
        });
        seedContext.SustainableMaterialCertifications.Add(new SustainableMaterialCertification
        {
            Id = Guid.NewGuid(), SustainabilityProfileId = profile.Id, MaterialName = "Cotton", CertifyingBody = "GOTS",
            CertificateReference = "x", IssuedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow,
        });
        await seedContext.SaveChangesAsync(Ct);

        var repository = new SustainabilityRepository(scope.NewContext());
        var found = await repository.GetByIdAsync(profile.Id, Ct);

        Assert.NotNull(found);
        Assert.Single(found!.MaterialRecords);
        Assert.Single(found.Certifications);
    }

    [Fact]
    public async Task GetByProducerIdAsync_UnknownProducer_ReturnsNull()
    {
        await using var scope = await _database.BeginAsync();
        var repository = new SustainabilityRepository(scope.NewContext());

        Assert.Null(await repository.GetByProducerIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetByProducerIdAsync_KnownProducer_ReturnsIt()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var profile = await SeedProfileAsync(seedContext);

        var repository = new SustainabilityRepository(scope.NewContext());
        var found = await repository.GetByProducerIdAsync(profile.ProducerId, Ct);

        Assert.Equal(profile.Id, found!.Id);
    }

    [Fact]
    public async Task GetCertificationByIdAsync_UnknownId_ReturnsNull()
    {
        await using var scope = await _database.BeginAsync();
        var repository = new SustainabilityRepository(scope.NewContext());

        Assert.Null(await repository.GetCertificationByIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetCertificationByIdAsync_KnownId_ReturnsItWithProfileLoaded()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var profile = await SeedProfileAsync(seedContext);
        var certification = new SustainableMaterialCertification
        {
            Id = Guid.NewGuid(), SustainabilityProfileId = profile.Id, MaterialName = "Cotton", CertifyingBody = "GOTS",
            CertificateReference = "x", IssuedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow,
        };
        seedContext.SustainableMaterialCertifications.Add(certification);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new SustainabilityRepository(scope.NewContext());
        var found = await repository.GetCertificationByIdAsync(certification.Id, Ct);

        Assert.NotNull(found);
        Assert.Equal(profile.Id, found!.SustainabilityProfile.Id);
    }

    [Fact]
    public async Task AddAsync_SecondProfileForTheSameProducer_ViolatesTheUniqueIndex()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var profile = await SeedProfileAsync(seedContext);

        var repository = new SustainabilityRepository(scope.NewContext());
        await repository.AddAsync(new SustainabilityProfile
        {
            Id = Guid.NewGuid(), ProducerId = profile.ProducerId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        }, Ct);

        await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync(Ct));
    }

    [Fact]
    public async Task AddMaterialRecordAsync_ThenSaveChangesAsync_PersistsTheRecord()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var profile = await SeedProfileAsync(seedContext);

        var repository = new SustainabilityRepository(scope.NewContext());
        await repository.AddMaterialRecordAsync(new SustainableMaterialRecord
        {
            Id = Guid.NewGuid(), SustainabilityProfileId = profile.Id, MaterialName = "Cotton", QuantityUsed = 1, Unit = "kg", RecordedAt = DateTime.UtcNow,
        }, Ct);
        await repository.SaveChangesAsync(Ct);

        var reloaded = await new SustainabilityRepository(scope.NewContext()).GetByIdAsync(profile.Id, Ct);
        Assert.Single(reloaded!.MaterialRecords);
    }

    [Fact]
    public async Task AddCertificationAsync_ThenSaveChangesAsync_PersistsTheCertification()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var profile = await SeedProfileAsync(seedContext);

        var repository = new SustainabilityRepository(scope.NewContext());
        await repository.AddCertificationAsync(new SustainableMaterialCertification
        {
            Id = Guid.NewGuid(), SustainabilityProfileId = profile.Id, MaterialName = "Cotton", CertifyingBody = "GOTS",
            CertificateReference = "x", IssuedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow,
        }, Ct);
        await repository.SaveChangesAsync(Ct);

        var reloaded = await new SustainabilityRepository(scope.NewContext()).GetByIdAsync(profile.Id, Ct);
        Assert.Single(reloaded!.Certifications);
    }
}
