using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.HeritageIdentity;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.ProducerComparison.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "ProducerComparison")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class ProducerComparisonRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public ProducerComparisonRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<User> SeedProducerAsync(ShilpoHubDbContext context)
    {
        var producerRole = await context.Roles.SingleAsync(r => r.Name == RoleNames.Producer, Ct);
        var producer = TestUsers.Create().WithRoles(producerRole);
        context.Users.Add(producer);
        await context.SaveChangesAsync(Ct);
        return producer;
    }

    private static async Task<Product> SeedProductAsync(ShilpoHubDbContext context, Guid producerId, decimal price, int stock = 1,
        int reviewCount = 0, decimal averageRating = 0, HandmadeVerificationStatus handmade = HandmadeVerificationStatus.Pending)
    {
        var district = await context.Districts.FirstAsync(Ct);
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category " + Guid.NewGuid().ToString("N")[..8], Slug = Guid.NewGuid().ToString("N") };
        var product = new Product
        {
            Id = Guid.NewGuid(), Name = "Nakshi Kantha", Slug = Guid.NewGuid().ToString("N"), Price = price, Stock = stock,
            CategoryId = category.Id, DistrictId = district.Id, ProducerId = producerId, IsActive = true,
            ReviewCount = reviewCount, AverageRating = averageRating, HandmadeVerificationStatus = handmade,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        context.Categories.Add(category);
        context.Products.Add(product);
        await context.SaveChangesAsync(Ct);
        return product;
    }

    [Fact]
    public async Task CompareAsync_UnknownId_ExcludesItFromTheResult()
    {
        await using var scope = await _database.BeginAsync();
        var repository = new ProducerComparisonRepository(scope.NewContext());

        var result = await repository.CompareAsync([Guid.NewGuid()], Ct);

        Assert.Empty(result);
    }

    [Fact]
    public async Task CompareAsync_UserWithoutProducerRole_IsExcluded()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var user = TestUsers.Create();
        seedContext.Users.Add(user);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new ProducerComparisonRepository(scope.NewContext());
        var result = await repository.CompareAsync([user.Id], Ct);

        Assert.Empty(result);
    }

    [Fact]
    public async Task CompareAsync_ProducerWithNoProducts_ReturnsARowWithZeroedAggregates()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var producer = await SeedProducerAsync(seedContext);

        var repository = new ProducerComparisonRepository(scope.NewContext());
        var row = Assert.Single(await repository.CompareAsync([producer.Id], Ct));

        Assert.Equal(producer.FullName, row.ProducerName);
        Assert.Equal(0, row.ProductCount);
        Assert.Null(row.MinPrice);
        Assert.Equal(0m, row.AverageRating);
        Assert.Equal(0m, row.HandmadeVerifiedRatio);
    }

    [Fact]
    public async Task CompareAsync_ProducerWithProducts_ComputesMinMaxAndAveragePrice()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var producer = await SeedProducerAsync(seedContext);
        await SeedProductAsync(seedContext, producer.Id, 100);
        await SeedProductAsync(seedContext, producer.Id, 300);

        var repository = new ProducerComparisonRepository(scope.NewContext());
        var row = Assert.Single(await repository.CompareAsync([producer.Id], Ct));

        Assert.Equal(2, row.ProductCount);
        Assert.Equal(100, row.MinPrice);
        Assert.Equal(300, row.MaxPrice);
        Assert.Equal(200, row.AveragePrice);
    }

    [Fact]
    public async Task CompareAsync_AverageRating_IsWeightedByReviewCount()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var producer = await SeedProducerAsync(seedContext);
        await SeedProductAsync(seedContext, producer.Id, 100, reviewCount: 1, averageRating: 5m);
        await SeedProductAsync(seedContext, producer.Id, 100, reviewCount: 9, averageRating: 2m);

        var repository = new ProducerComparisonRepository(scope.NewContext());
        var row = Assert.Single(await repository.CompareAsync([producer.Id], Ct));

        // (5*1 + 2*9) / 10 = 23/10 = 2.3
        Assert.Equal(2.3m, row.AverageRating);
        Assert.Equal(10, row.TotalReviewCount);
    }

    [Fact]
    public async Task CompareAsync_HandmadeVerifiedRatio_IsVerifiedCountOverProductCount()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var producer = await SeedProducerAsync(seedContext);
        await SeedProductAsync(seedContext, producer.Id, 100, handmade: HandmadeVerificationStatus.Verified);
        await SeedProductAsync(seedContext, producer.Id, 100, handmade: HandmadeVerificationStatus.Verified);
        await SeedProductAsync(seedContext, producer.Id, 100, handmade: HandmadeVerificationStatus.Pending);
        await SeedProductAsync(seedContext, producer.Id, 100, handmade: HandmadeVerificationStatus.Rejected);

        var repository = new ProducerComparisonRepository(scope.NewContext());
        var row = Assert.Single(await repository.CompareAsync([producer.Id], Ct));

        Assert.Equal(2, row.HandmadeVerifiedProductCount);
        Assert.Equal(0.5m, row.HandmadeVerifiedRatio);
    }

    [Fact]
    public async Task CompareAsync_InactiveProducts_AreExcludedFromAggregates()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var producer = await SeedProducerAsync(seedContext);
        var product = await SeedProductAsync(seedContext, producer.Id, 100);
        product.IsActive = false;
        await seedContext.SaveChangesAsync(Ct);

        var repository = new ProducerComparisonRepository(scope.NewContext());
        var row = Assert.Single(await repository.CompareAsync([producer.Id], Ct));

        Assert.Equal(0, row.ProductCount);
    }

    [Fact]
    public async Task CompareAsync_ProducerWithHeritageIdentity_IncludesWorkshopAndDistrictInfo()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var producer = await SeedProducerAsync(seedContext);
        var district = await seedContext.Districts.FirstAsync(Ct);
        seedContext.ProducerHeritageIdentities.Add(new ProducerHeritageIdentity
        {
            Id = Guid.NewGuid(), ProducerId = producer.Id, HeritageIdNumber = "HID-" + Guid.NewGuid().ToString("N")[..8],
            PrimaryCraft = "Weaving", WorkshopName = "Dhaka Looms", DistrictId = district.Id,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });
        await seedContext.SaveChangesAsync(Ct);

        var repository = new ProducerComparisonRepository(scope.NewContext());
        var row = Assert.Single(await repository.CompareAsync([producer.Id], Ct));

        Assert.Equal("Weaving", row.PrimaryCraft);
        Assert.Equal("Dhaka Looms", row.WorkshopName);
        Assert.Equal(district.Name, row.DistrictName);
    }

    [Fact]
    public async Task CompareAsync_MultipleIds_ReturnsOneRowPerFoundProducer_InRequestOrder()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var first = await SeedProducerAsync(seedContext);
        var second = await SeedProducerAsync(seedContext);

        var repository = new ProducerComparisonRepository(scope.NewContext());
        var result = await repository.CompareAsync([first.Id, second.Id], Ct);

        Assert.Equal(new[] { first.Id, second.Id }, result.Select(r => r.ProducerId));
    }
}
