using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;
using CertificateEntity = ShilpoHubBD.Domain.Entities.Certificate.Certificate;

namespace ShilpoHubBD.UnitTests.Features.Certificate.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Certificate")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class CertificateRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public CertificateRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<(User Producer, Product Product)> SeedProductAsync(ShilpoHubDbContext context)
    {
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category " + Guid.NewGuid().ToString("N")[..8], Slug = Guid.NewGuid().ToString("N") };
        var district = await context.Districts.FirstAsync(Ct);
        var producer = TestUsers.Create();
        var product = new Product
        {
            Id = Guid.NewGuid(), Name = "Jamdani Saree", Slug = Guid.NewGuid().ToString("N"), Price = 1000, Stock = 1,
            CategoryId = category.Id, DistrictId = district.Id, ProducerId = producer.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        context.Categories.Add(category);
        context.Users.Add(producer);
        context.Products.Add(product);
        await context.SaveChangesAsync(Ct);
        return (producer, product);
    }

    private static CertificateEntity MakeCertificate(Product product, User producer, string? number = null, bool revoked = false, DateTime? issuedAt = null) => new()
    {
        Id = Guid.NewGuid(), ProductId = product.Id, ProducerId = producer.Id, CertificateNumber = number ?? $"SH-{Guid.NewGuid():N}"[..20],
        ProductName = product.Name, ProducerName = producer.FullName, District = "Dhaka", Category = "Weaving",
        IsRevoked = revoked, IssuedAt = issuedAt ?? DateTime.UtcNow,
    };

    [Fact]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsTheCertificate()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (producer, product) = await SeedProductAsync(context);
        var certificate = MakeCertificate(product, producer);
        var repository = new CertificateRepository(db.NewContext());

        await repository.AddAsync(certificate, Ct);
        await repository.SaveChangesAsync(Ct);

        Assert.True(await db.NewContext().Certificates.AnyAsync(c => c.Id == certificate.Id, Ct));
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var db = await _database.BeginAsync();

        Assert.Null(await new CertificateRepository(db.NewContext()).GetByIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetByCertificateNumberAsync_ExactMatchOnly()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (producer, product) = await SeedProductAsync(context);
        var number = $"SH-{Guid.NewGuid():N}"[..20];
        context.Certificates.Add(MakeCertificate(product, producer, number));
        await context.SaveChangesAsync(Ct);
        var repository = new CertificateRepository(db.NewContext());

        Assert.NotNull(await repository.GetByCertificateNumberAsync(number, Ct));
        Assert.Null(await repository.GetByCertificateNumberAsync(number.ToLowerInvariant(), Ct));
    }

    [Fact]
    public async Task GetActiveByProductIdAsync_RevokedCertificate_IsNotConsideredActive()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (producer, product) = await SeedProductAsync(context);
        context.Certificates.Add(MakeCertificate(product, producer, revoked: true));
        await context.SaveChangesAsync(Ct);

        Assert.Null(await new CertificateRepository(db.NewContext()).GetActiveByProductIdAsync(product.Id, Ct));
    }

    [Fact]
    public async Task GetActiveByProductIdAsync_ActiveCertificate_IsReturned()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (producer, product) = await SeedProductAsync(context);
        var certificate = MakeCertificate(product, producer);
        context.Certificates.Add(certificate);
        await context.SaveChangesAsync(Ct);

        var found = await new CertificateRepository(db.NewContext()).GetActiveByProductIdAsync(product.Id, Ct);

        Assert.Equal(certificate.Id, found!.Id);
    }

    [Fact]
    public async Task GetByProducerAsync_ReturnsNewestFirst()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (producer, product) = await SeedProductAsync(context);
        var older = MakeCertificate(product, producer, issuedAt: DateTime.UtcNow.AddDays(-1));
        var newer = MakeCertificate(product, producer, issuedAt: DateTime.UtcNow);
        context.Certificates.AddRange(older, newer);
        await context.SaveChangesAsync(Ct);

        var certificates = await new CertificateRepository(db.NewContext()).GetByProducerAsync(producer.Id, Ct);

        Assert.Equal(new[] { newer.Id, older.Id }, certificates.Select(c => c.Id));
    }

    [Fact]
    public async Task GetByProducerAsync_AnotherProducersCertificates_AreExcluded()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (producer, product) = await SeedProductAsync(context);
        var (otherProducer, otherProduct) = await SeedProductAsync(context);
        context.Certificates.AddRange(MakeCertificate(product, producer), MakeCertificate(otherProduct, otherProducer));
        await context.SaveChangesAsync(Ct);

        var certificates = await new CertificateRepository(db.NewContext()).GetByProducerAsync(producer.Id, Ct);

        Assert.All(certificates, c => Assert.Equal(producer.Id, c.ProducerId));
    }
}
