using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Entities.Certificate;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.Reviews;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Certificates.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Certificates")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class ExpertiseCertificateRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public ExpertiseCertificateRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<(Category Category, District District)> SeedReferenceDataAsync(ShilpoHubDbContext context)
    {
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category " + Guid.NewGuid().ToString("N")[..8], Slug = Guid.NewGuid().ToString("N") };
        var district = await context.Districts.FirstAsync(Ct);
        context.Categories.Add(category);
        return (category, district);
    }

    private static Product MakeProduct(User producer, Category category, District district, string name) => new()
    {
        Id = Guid.NewGuid(), Name = name, Slug = Guid.NewGuid().ToString("N"), ProducerId = producer.Id, CategoryId = category.Id,
        DistrictId = district.Id, Price = 100, Stock = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
    };

    private static Review MakeReview(User reviewer, Product product, int rating, int? producerRating = null) => new()
    {
        Id = Guid.NewGuid(), UserId = reviewer.Id, ProductId = product.Id, Rating = rating, ProducerRating = producerRating,
        Comment = "x", CreatedAt = DateTime.UtcNow,
    };

    private static ExpertiseCertificate MakeCertificate(User producer, User admin, string number, bool revoked = false) => new()
    {
        Id = Guid.NewGuid(), ProducerId = producer.Id, CertificateNumber = number, Level = ExpertiseLevel.Bronze, Expertise = "x",
        AverageRating = 4.5m, RatingCount = 5, IssuedByUserId = admin.Id, IssuedAt = DateTime.UtcNow, IsRevoked = revoked,
    };

    // ---------- GetRatingStatsAsync ----------

    [Fact]
    public async Task GetRatingStatsAsync_GroupsReviewsByProducerAndAveragesTheRating()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (category, district) = await SeedReferenceDataAsync(context);
        var producer = TestUsers.Create(fullName: "Rahima Begum");
        var customerA = TestUsers.Create();
        var customerB = TestUsers.Create();
        context.Users.AddRange(producer, customerA, customerB);
        var product = MakeProduct(producer, category, district, "Jamdani Saree");
        context.Products.Add(product);
        context.Reviews.AddRange(MakeReview(customerA, product, 5), MakeReview(customerB, product, 3));
        await context.SaveChangesAsync(Ct);

        var stats = await new ExpertiseCertificateRepository(db.NewContext()).GetRatingStatsAsync(producer.Id, Ct);

        var stat = Assert.Single(stats);
        Assert.Equal("Rahima Begum", stat.ProducerName);
        Assert.Equal(2, stat.Count);
        Assert.Equal(4.0, stat.Average, 3);
    }

    [Fact]
    public async Task GetRatingStatsAsync_PrefersProducerRatingOverTheGeneralRatingWhenBothArePresent()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (category, district) = await SeedReferenceDataAsync(context);
        var producer = TestUsers.Create();
        var customer = TestUsers.Create();
        context.Users.AddRange(producer, customer);
        var product = MakeProduct(producer, category, district, "Nakshi Kantha");
        context.Products.Add(product);
        context.Reviews.Add(MakeReview(customer, product, rating: 2, producerRating: 5));
        await context.SaveChangesAsync(Ct);

        var stat = (await new ExpertiseCertificateRepository(db.NewContext()).GetRatingStatsAsync(producer.Id, Ct)).Single();

        Assert.Equal(5.0, stat.Average, 3);
    }

    [Fact]
    public async Task GetRatingStatsAsync_ReviewWithoutAProduct_IsExcluded()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var reviewer = TestUsers.Create();
        context.Users.Add(reviewer);
        context.Reviews.Add(new Review { Id = Guid.NewGuid(), UserId = reviewer.Id, ProductId = null, Rating = 5, Comment = "x", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync(Ct);

        var stats = await new ExpertiseCertificateRepository(db.NewContext()).GetRatingStatsAsync(null, Ct);

        Assert.DoesNotContain(stats, s => s.ProducerId == reviewer.Id);
    }

    [Fact]
    public async Task GetRatingStatsAsync_NoProducerIdFilter_ReturnsEveryProducer()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (category, district) = await SeedReferenceDataAsync(context);
        var producerA = TestUsers.Create();
        var producerB = TestUsers.Create();
        var customer = TestUsers.Create();
        context.Users.AddRange(producerA, producerB, customer);
        var productA = MakeProduct(producerA, category, district, "A");
        var productB = MakeProduct(producerB, category, district, "B");
        context.Products.AddRange(productA, productB);
        context.Reviews.AddRange(MakeReview(customer, productA, 4), MakeReview(customer, productB, 4));
        await context.SaveChangesAsync(Ct);

        var stats = await new ExpertiseCertificateRepository(db.NewContext()).GetRatingStatsAsync(null, Ct);

        Assert.Contains(stats, s => s.ProducerId == producerA.Id);
        Assert.Contains(stats, s => s.ProducerId == producerB.Id);
    }

    // ---------- GetByProducerAsync / GetAllActiveAsync ----------

    [Fact]
    public async Task GetByProducerAsync_ReturnsNewestFirstWithTheProducerLoaded()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var producer = TestUsers.Create(fullName: "Rahima Begum");
        var admin = TestUsers.Create();
        context.Users.AddRange(producer, admin);
        var older = MakeCertificate(producer, admin, $"EXP-{Guid.NewGuid():N}"[..15]);
        older.IssuedAt = DateTime.UtcNow.AddDays(-1);
        var newer = MakeCertificate(producer, admin, $"EXP-{Guid.NewGuid():N}"[..15]);
        newer.IssuedAt = DateTime.UtcNow;
        context.ExpertiseCertificates.AddRange(older, newer);
        await context.SaveChangesAsync(Ct);

        var certificates = await new ExpertiseCertificateRepository(db.NewContext()).GetByProducerAsync(producer.Id, Ct);

        Assert.Equal(new[] { newer.Id, older.Id }, certificates.Select(c => c.Id));
        Assert.Equal("Rahima Begum", certificates[0].Producer.FullName);
    }

    [Fact]
    public async Task GetAllActiveAsync_ExcludesRevokedCertificates()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var producer = TestUsers.Create();
        var admin = TestUsers.Create();
        context.Users.AddRange(producer, admin);
        var active = MakeCertificate(producer, admin, $"EXP-{Guid.NewGuid():N}"[..15]);
        var revoked = MakeCertificate(producer, admin, $"EXP-{Guid.NewGuid():N}"[..15], revoked: true);
        context.ExpertiseCertificates.AddRange(active, revoked);
        await context.SaveChangesAsync(Ct);

        var certificates = await new ExpertiseCertificateRepository(db.NewContext()).GetAllActiveAsync(Ct);

        Assert.Contains(certificates, c => c.Id == active.Id);
        Assert.DoesNotContain(certificates, c => c.Id == revoked.Id);
    }

    // ---------- GetProfileAsync ----------

    [Fact]
    public async Task GetProfileAsync_ApprovedProfile_ReturnsExpertiseAndApprovedTrue()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var producer = TestUsers.Create();
        context.Users.Add(producer);
        context.UserProfiles.Add(new UserProfile
        {
            Id = Guid.NewGuid(), UserId = producer.Id, LegalName = "x", Phone = "x", NidNumber = Guid.NewGuid().ToString("N")[..10],
            AddressLine = "x", Expertise = "Jamdani weaving", Status = ShilpoHubBD.Domain.Entities.Identity.UserProfileStatus.Approved,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync(Ct);

        var (expertise, approved) = await new ExpertiseCertificateRepository(db.NewContext()).GetProfileAsync(producer.Id, Ct);

        Assert.Equal("Jamdani weaving", expertise);
        Assert.True(approved);
    }

    [Fact]
    public async Task GetProfileAsync_NoProfile_ReturnsNullAndNotApproved()
    {
        await using var db = await _database.BeginAsync();

        var (expertise, approved) = await new ExpertiseCertificateRepository(db.NewContext()).GetProfileAsync(Guid.NewGuid(), Ct);

        Assert.Null(expertise);
        Assert.False(approved);
    }

    // ---------- NumberExistsAsync / AddAsync ----------

    [Fact]
    public async Task NumberExistsAsync_ReflectsWhetherThatNumberIsTaken()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var producer = TestUsers.Create();
        var admin = TestUsers.Create();
        context.Users.AddRange(producer, admin);
        var number = $"EXP-{Guid.NewGuid():N}"[..15];
        context.ExpertiseCertificates.Add(MakeCertificate(producer, admin, number));
        await context.SaveChangesAsync(Ct);
        var repository = new ExpertiseCertificateRepository(db.NewContext());

        Assert.True(await repository.NumberExistsAsync(number, Ct));
        Assert.False(await repository.NumberExistsAsync("EXP-DOES-NOT-EXIST", Ct));
    }

    [Fact]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsTheCertificate()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var producer = TestUsers.Create();
        var admin = TestUsers.Create();
        context.Users.AddRange(producer, admin);
        await context.SaveChangesAsync(Ct);
        var certificate = MakeCertificate(producer, admin, $"EXP-{Guid.NewGuid():N}"[..15]);
        var repository = new ExpertiseCertificateRepository(db.NewContext());

        await repository.AddAsync(certificate, Ct);
        await repository.SaveChangesAsync(Ct);

        Assert.True(await db.NewContext().ExpertiseCertificates.AnyAsync(c => c.Id == certificate.Id, Ct));
    }
}
