using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Application.DTOs.SupplierDiscovery;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.SupplierDiscovery;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.SupplierDiscovery.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "SupplierDiscovery")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class SupplierDiscoveryRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public SupplierDiscoveryRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static SupplierSearchParameters Params(int page = 1, int pageSize = 20) => new() { Page = page, PageSize = pageSize };

    private static async Task<Product> SeedProductAsync(ShilpoHubDbContext context, decimal price, int stock = 1,
        int reviewCount = 0, decimal averageRating = 0, bool isActive = true, Guid? categoryId = null, Guid? producerId = null)
    {
        var district = await context.Districts.FirstAsync(Ct);
        var category = categoryId is null
            ? new Category { Id = Guid.NewGuid(), Name = "Test Category " + Guid.NewGuid().ToString("N")[..8], Slug = Guid.NewGuid().ToString("N") }
            : null;
        if (category is not null)
        {
            context.Categories.Add(category);
        }

        Guid producer;
        if (producerId is { } id)
        {
            producer = id;
        }
        else
        {
            var producerUser = TestUsers.Create();
            context.Users.Add(producerUser);
            producer = producerUser.Id;
        }

        var product = new Product
        {
            Id = Guid.NewGuid(), Name = "Nakshi Kantha " + Guid.NewGuid().ToString("N")[..8], Slug = Guid.NewGuid().ToString("N"), Price = price, Stock = stock,
            CategoryId = categoryId ?? category!.Id, DistrictId = district.Id, ProducerId = producer, IsActive = isActive,
            ReviewCount = reviewCount, AverageRating = averageRating, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        context.Products.Add(product);
        await context.SaveChangesAsync(Ct);
        return product;
    }

    [Fact]
    public async Task SearchAsync_NoFilters_ReturnsOneRowPerProducerWithActiveProducts()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        await SeedProductAsync(seedContext, 100);
        await SeedProductAsync(seedContext, 200, isActive: false);

        var repository = new SupplierDiscoveryRepository(scope.NewContext());
        var (items, totalCount) = await repository.SearchAsync(Params(), Ct);

        Assert.Equal(1, totalCount);
        Assert.Single(items);
    }

    [Fact]
    public async Task SearchAsync_FiltersByCategoryId()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var inCategory = await SeedProductAsync(seedContext, 100);
        await SeedProductAsync(seedContext, 100);

        var repository = new SupplierDiscoveryRepository(scope.NewContext());
        var (items, _) = await repository.SearchAsync(new SupplierSearchParameters { CategoryId = inCategory.CategoryId, Page = 1, PageSize = 20 }, Ct);

        Assert.Equal(inCategory.ProducerId, Assert.Single(items).ProducerId);
    }

    [Fact]
    public async Task SearchAsync_FiltersByPriceRange()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var cheap = await SeedProductAsync(seedContext, 50);
        var expensive = await SeedProductAsync(seedContext, 5000);

        var repository = new SupplierDiscoveryRepository(scope.NewContext());
        var (items, _) = await repository.SearchAsync(new SupplierSearchParameters { MinPrice = 100, MaxPrice = 1000, Page = 1, PageSize = 20 }, Ct);

        Assert.Empty(items);
        Assert.NotEqual(cheap.ProducerId, expensive.ProducerId);
    }

    [Fact]
    public async Task SearchAsync_FiltersByMinRating()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var lowRated = await SeedProductAsync(seedContext, 100, reviewCount: 5, averageRating: 2m);
        var highRated = await SeedProductAsync(seedContext, 100, reviewCount: 5, averageRating: 4.5m);

        var repository = new SupplierDiscoveryRepository(scope.NewContext());
        var (items, _) = await repository.SearchAsync(new SupplierSearchParameters { MinRating = 4, Page = 1, PageSize = 20 }, Ct);

        Assert.Equal(highRated.ProducerId, Assert.Single(items).ProducerId);
        Assert.NotEqual(lowRated.ProducerId, highRated.ProducerId);
    }

    [Fact]
    public async Task SearchAsync_FiltersByMinProductionCapacity()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var lowStock = await SeedProductAsync(seedContext, 100, stock: 2);
        var highStock = await SeedProductAsync(seedContext, 100, stock: 50);

        var repository = new SupplierDiscoveryRepository(scope.NewContext());
        var (items, _) = await repository.SearchAsync(new SupplierSearchParameters { MinProductionCapacity = 10, Page = 1, PageSize = 20 }, Ct);

        Assert.Equal(highStock.ProducerId, Assert.Single(items).ProducerId);
        Assert.NotEqual(lowStock.ProducerId, highStock.ProducerId);
    }

    [Fact]
    public async Task SearchAsync_HandmadeVerifiedOnly_ExcludesUnverifiedProducers()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var unverified = await SeedProductAsync(seedContext, 100);
        var verifiedProduct = await SeedProductAsync(seedContext, 100);
        verifiedProduct.HandmadeVerificationStatus = HandmadeVerificationStatus.Verified;
        await seedContext.SaveChangesAsync(Ct);

        var repository = new SupplierDiscoveryRepository(scope.NewContext());
        var (items, _) = await repository.SearchAsync(new SupplierSearchParameters { HandmadeVerifiedOnly = true, Page = 1, PageSize = 20 }, Ct);

        Assert.Equal(verifiedProduct.ProducerId, Assert.Single(items).ProducerId);
        Assert.NotEqual(unverified.ProducerId, verifiedProduct.ProducerId);
    }

    [Fact]
    public async Task SearchAsync_SortByPriceLowToHigh_OrdersByMinPriceAscending()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var expensive = await SeedProductAsync(seedContext, 900);
        var cheap = await SeedProductAsync(seedContext, 100);

        var repository = new SupplierDiscoveryRepository(scope.NewContext());
        var (items, _) = await repository.SearchAsync(new SupplierSearchParameters { SortBy = SupplierSortOption.PriceLowToHigh, Page = 1, PageSize = 20 }, Ct);

        Assert.Equal(new[] { cheap.ProducerId, expensive.ProducerId }, items.Select(i => i.ProducerId));
    }

    [Fact]
    public async Task SearchAsync_Paging_SplitsResultsAcrossPages()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        await SeedProductAsync(seedContext, 100);
        await SeedProductAsync(seedContext, 200);
        await SeedProductAsync(seedContext, 300);

        var repository = new SupplierDiscoveryRepository(scope.NewContext());
        var (page1, total) = await repository.SearchAsync(Params(page: 1, pageSize: 2), Ct);
        var (page2, _) = await repository.SearchAsync(Params(page: 2, pageSize: 2), Ct);

        Assert.Equal(3, total);
        Assert.Equal(2, page1.Count);
        Assert.Single(page2);
    }

    [Fact]
    public async Task SearchAsync_ComputesAverageRatingAsWeightedByReviewCount()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var producerUser = TestUsers.Create();
        seedContext.Users.Add(producerUser);
        await seedContext.SaveChangesAsync(Ct);
        await SeedProductAsync(seedContext, 100, reviewCount: 1, averageRating: 5m, producerId: producerUser.Id);
        await SeedProductAsync(seedContext, 100, reviewCount: 9, averageRating: 2m, producerId: producerUser.Id);

        var repository = new SupplierDiscoveryRepository(scope.NewContext());
        var (items, _) = await repository.SearchAsync(Params(), Ct);

        Assert.Equal(2.3m, Assert.Single(items).AverageRating);
    }

    [Fact]
    public async Task GetProducerProfileAsync_UnknownProducer_ReturnsNull()
    {
        await using var scope = await _database.BeginAsync();
        var repository = new SupplierDiscoveryRepository(scope.NewContext());

        Assert.Null(await repository.GetProducerProfileAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetProducerProfileAsync_KnownProducerNoProducts_ReturnsAnEmptyShellProfile()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var producer = TestUsers.Create();
        seedContext.Users.Add(producer);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new SupplierDiscoveryRepository(scope.NewContext());
        var profile = await repository.GetProducerProfileAsync(producer.Id, Ct);

        Assert.NotNull(profile);
        Assert.Equal(producer.FullName, profile!.ProducerName);
        Assert.Equal(0, profile.ProductCount);
        Assert.Null(profile.MinPrice);
        Assert.Empty(profile.Products);
    }

    [Fact]
    public async Task GetProducerProfileAsync_KnownProducerWithProducts_AggregatesPriceAndRating()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var producer = TestUsers.Create();
        seedContext.Users.Add(producer);
        await seedContext.SaveChangesAsync(Ct);
        await SeedProductAsync(seedContext, 100, reviewCount: 2, averageRating: 4m, producerId: producer.Id);
        await SeedProductAsync(seedContext, 500, reviewCount: 0, producerId: producer.Id);

        var repository = new SupplierDiscoveryRepository(scope.NewContext());
        var profile = await repository.GetProducerProfileAsync(producer.Id, Ct);

        Assert.Equal(2, profile!.ProductCount);
        Assert.Equal(100, profile.MinPrice);
        Assert.Equal(500, profile.MaxPrice);
        Assert.Equal(4m, profile.AverageRating);
        Assert.Equal(2, profile.TotalReviewCount);
    }
}
