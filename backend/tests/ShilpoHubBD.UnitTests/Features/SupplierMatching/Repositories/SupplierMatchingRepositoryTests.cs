using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Application.DTOs.SupplierMatching;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.Sustainability;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.SupplierMatching.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "SupplierMatching")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class SupplierMatchingRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public SupplierMatchingRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<(Product Product, Guid ProducerId)> SeedProductAsync(ShilpoHubDbContext context, decimal price = 100,
        string name = "Nakshi Kantha", Guid? categoryId = null, Guid? districtOverride = null)
    {
        var district = districtOverride is { } dId ? await context.Districts.FirstAsync(d => d.Id == dId, Ct) : await context.Districts.FirstAsync(Ct);
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category " + Guid.NewGuid().ToString("N")[..8], Slug = Guid.NewGuid().ToString("N") };
        var producer = TestUsers.Create();
        var product = new Product
        {
            Id = Guid.NewGuid(), Name = name, Slug = Guid.NewGuid().ToString("N"), Description = "x", Price = price, Stock = 1,
            CategoryId = categoryId ?? category.Id, DistrictId = district.Id, ProducerId = producer.Id, IsActive = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        context.Categories.Add(category);
        context.Users.Add(producer);
        context.Products.Add(product);
        await context.SaveChangesAsync(Ct);
        return (product, producer.Id);
    }

    private static SupplierMatchCandidateDto FindCandidate(List<SupplierMatchCandidateDto> candidates, Guid producerId)
        => candidates.Single(c => c.ProducerId == producerId);

    [Fact]
    public async Task GetCandidatesAsync_NoProducts_ReturnsEmpty()
    {
        await using var scope = await _database.BeginAsync();
        var repository = new SupplierMatchingRepository(scope.NewContext());

        var result = await repository.GetCandidatesAsync(new SupplierMatchRequest(), Ct);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCandidatesAsync_CategoryIdMatches_SetsHasMatchingCategory()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var (product, producerId) = await SeedProductAsync(seedContext);

        var repository = new SupplierMatchingRepository(scope.NewContext());
        var result = await repository.GetCandidatesAsync(new SupplierMatchRequest { CategoryId = product.CategoryId }, Ct);

        Assert.True(FindCandidate(result, producerId).HasMatchingCategory);
    }

    [Fact]
    public async Task GetCandidatesAsync_CategoryIdDoesNotMatch_LeavesItUnmatched()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var (_, producerId) = await SeedProductAsync(seedContext);

        var repository = new SupplierMatchingRepository(scope.NewContext());
        var result = await repository.GetCandidatesAsync(new SupplierMatchRequest { CategoryId = Guid.NewGuid() }, Ct);

        Assert.False(FindCandidate(result, producerId).HasMatchingCategory);
    }

    [Fact]
    public async Task GetCandidatesAsync_DistrictIdMatches_SetsHasMatchingDistrict()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var (product, producerId) = await SeedProductAsync(seedContext);

        var repository = new SupplierMatchingRepository(scope.NewContext());
        var result = await repository.GetCandidatesAsync(new SupplierMatchRequest { DistrictId = product.DistrictId }, Ct);

        Assert.True(FindCandidate(result, producerId).HasMatchingDistrict);
    }

    [Fact]
    public async Task GetCandidatesAsync_KeywordMatchesProductNameCaseInsensitively_SetsHasMatchingKeyword()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var (_, producerId) = await SeedProductAsync(seedContext, name: "Handwoven Nakshi Kantha");

        var repository = new SupplierMatchingRepository(scope.NewContext());
        var result = await repository.GetCandidatesAsync(new SupplierMatchRequest { ProductKeyword = "kantha" }, Ct);

        Assert.True(FindCandidate(result, producerId).HasMatchingKeyword);
    }

    [Fact]
    public async Task GetCandidatesAsync_BudgetAtOrAboveTheCheapestProduct_SetsHasProductWithinBudget()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var (_, producerId) = await SeedProductAsync(seedContext, price: 100);

        var repository = new SupplierMatchingRepository(scope.NewContext());
        var result = await repository.GetCandidatesAsync(new SupplierMatchRequest { MaxBudgetPerUnit = 150 }, Ct);

        Assert.True(FindCandidate(result, producerId).HasProductWithinBudget);
    }

    [Fact]
    public async Task GetCandidatesAsync_BudgetBelowEveryProduct_LeavesItUnmatched()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var (_, producerId) = await SeedProductAsync(seedContext, price: 500);

        var repository = new SupplierMatchingRepository(scope.NewContext());
        var result = await repository.GetCandidatesAsync(new SupplierMatchRequest { MaxBudgetPerUnit = 100 }, Ct);

        Assert.False(FindCandidate(result, producerId).HasProductWithinBudget);
    }

    [Fact]
    public async Task GetCandidatesAsync_MaterialMatchesASustainabilityRecord_SetsHasMatchingMaterial()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var (_, producerId) = await SeedProductAsync(seedContext);
        var profile = new SustainabilityProfile { Id = Guid.NewGuid(), ProducerId = producerId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        seedContext.SustainabilityProfiles.Add(profile);
        seedContext.SustainableMaterialRecords.Add(new SustainableMaterialRecord
        {
            Id = Guid.NewGuid(), SustainabilityProfileId = profile.Id, MaterialName = "Organic Cotton", Unit = "kg", RecordedAt = DateTime.UtcNow,
        });
        await seedContext.SaveChangesAsync(Ct);

        var repository = new SupplierMatchingRepository(scope.NewContext());
        var result = await repository.GetCandidatesAsync(new SupplierMatchRequest { Material = "cotton" }, Ct);

        Assert.True(FindCandidate(result, producerId).HasMatchingMaterial);
    }

    [Fact]
    public async Task GetCandidatesAsync_NoMaterialRequested_LeavesHasMatchingMaterialFalse()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var (_, producerId) = await SeedProductAsync(seedContext);

        var repository = new SupplierMatchingRepository(scope.NewContext());
        var result = await repository.GetCandidatesAsync(new SupplierMatchRequest(), Ct);

        Assert.False(FindCandidate(result, producerId).HasMatchingMaterial);
    }

    [Fact]
    public async Task GetCandidatesAsync_HandmadeVerifiedProduct_SetsIsHandmadeVerified()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var (product, producerId) = await SeedProductAsync(seedContext);
        product.HandmadeVerificationStatus = HandmadeVerificationStatus.Verified;
        await seedContext.SaveChangesAsync(Ct);

        var repository = new SupplierMatchingRepository(scope.NewContext());
        var result = await repository.GetCandidatesAsync(new SupplierMatchRequest(), Ct);

        Assert.True(FindCandidate(result, producerId).IsHandmadeVerified);
    }

    [Fact]
    public async Task GetCandidatesAsync_AggregatesProductCountAndMinPrice()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var (first, producerId) = await SeedProductAsync(seedContext, price: 300);
        var producer = await seedContext.Users.FirstAsync(u => u.Id == producerId, Ct);
        var district = await seedContext.Districts.FirstAsync(Ct);
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category " + Guid.NewGuid().ToString("N")[..8], Slug = Guid.NewGuid().ToString("N") };
        seedContext.Categories.Add(category);
        seedContext.Products.Add(new Product
        {
            Id = Guid.NewGuid(), Name = "Second item", Slug = Guid.NewGuid().ToString("N"), Description = "x", Price = 100, Stock = 1,
            CategoryId = category.Id, DistrictId = district.Id, ProducerId = producerId, IsActive = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });
        await seedContext.SaveChangesAsync(Ct);

        var repository = new SupplierMatchingRepository(scope.NewContext());
        var result = await repository.GetCandidatesAsync(new SupplierMatchRequest(), Ct);

        var candidate = FindCandidate(result, producerId);
        Assert.Equal(2, candidate.ProductCount);
        Assert.Equal(100, candidate.MinPrice);
        Assert.Equal(producer.FullName, candidate.ProducerName);
    }
}
