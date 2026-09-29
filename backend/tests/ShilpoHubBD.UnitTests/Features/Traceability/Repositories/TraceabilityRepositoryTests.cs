using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.Traceability;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Traceability.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Traceability")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class TraceabilityRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public TraceabilityRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<Product> SeedProductAsync(ShilpoHubDbContext context)
    {
        var district = await context.Districts.FirstAsync(Ct);
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category " + Guid.NewGuid().ToString("N")[..8], Slug = Guid.NewGuid().ToString("N") };
        var producer = TestUsers.Create();
        var product = new Product
        {
            Id = Guid.NewGuid(), Name = "Nakshi Kantha", Slug = Guid.NewGuid().ToString("N"), Price = 500, Stock = 1,
            CategoryId = category.Id, DistrictId = district.Id, ProducerId = producer.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        context.Categories.Add(category);
        context.Users.Add(producer);
        context.Products.Add(product);
        await context.SaveChangesAsync(Ct);
        return product;
    }

    private static ProductTraceability MakeTraceability(Product product) => new()
    {
        Id = Guid.NewGuid(), ProductId = product.Id, Summary = "Handwoven from local cotton.",
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        MaterialSources = { new MaterialSource { Id = Guid.NewGuid(), MaterialName = "Cotton", SourceLocation = "Rangpur", Description = "x", DisplayOrder = 0 } },
        TimelineEvents = { new TimelineEvent { Id = Guid.NewGuid(), Title = "Woven", Description = "x", EventDate = DateTime.UtcNow, DisplayOrder = 0 } },
    };

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var scope = await _database.BeginAsync();
        var repository = new TraceabilityRepository(scope.NewContext());

        Assert.Null(await repository.GetByIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetByIdAsync_KnownId_ReturnsItWithProductAndChildRowsLoaded()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var product = await SeedProductAsync(seedContext);
        var traceability = MakeTraceability(product);
        seedContext.ProductTraceabilities.Add(traceability);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new TraceabilityRepository(scope.NewContext());
        var found = await repository.GetByIdAsync(traceability.Id, Ct);

        Assert.NotNull(found);
        Assert.Equal(product.Name, found!.Product.Name);
        Assert.Single(found.MaterialSources);
        Assert.Single(found.TimelineEvents);
    }

    [Fact]
    public async Task GetByProductIdAsync_UnknownProduct_ReturnsNull()
    {
        await using var scope = await _database.BeginAsync();
        var repository = new TraceabilityRepository(scope.NewContext());

        Assert.Null(await repository.GetByProductIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetByProductIdAsync_KnownProduct_ReturnsTheTraceabilityRecord()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var product = await SeedProductAsync(seedContext);
        var traceability = MakeTraceability(product);
        seedContext.ProductTraceabilities.Add(traceability);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new TraceabilityRepository(scope.NewContext());
        var found = await repository.GetByProductIdAsync(product.Id, Ct);

        Assert.NotNull(found);
        Assert.Equal(traceability.Id, found!.Id);
    }

    [Fact]
    public async Task ExistsByProductIdAsync_ReflectsWhetherTheProductHasARecord()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var withRecord = await SeedProductAsync(seedContext);
        var withoutRecord = await SeedProductAsync(seedContext);
        seedContext.ProductTraceabilities.Add(MakeTraceability(withRecord));
        await seedContext.SaveChangesAsync(Ct);

        var repository = new TraceabilityRepository(scope.NewContext());

        Assert.True(await repository.ExistsByProductIdAsync(withRecord.Id, Ct));
        Assert.False(await repository.ExistsByProductIdAsync(withoutRecord.Id, Ct));
    }

    [Fact]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsTheRecordWithItsChildRows()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var product = await SeedProductAsync(seedContext);

        var repository = new TraceabilityRepository(scope.NewContext());
        var traceability = MakeTraceability(product);
        await repository.AddAsync(traceability, Ct);
        await repository.SaveChangesAsync(Ct);

        var reloaded = await new TraceabilityRepository(scope.NewContext()).GetByProductIdAsync(product.Id, Ct);
        Assert.NotNull(reloaded);
        Assert.Equal("Handwoven from local cotton.", reloaded!.Summary);
        Assert.Single(reloaded.MaterialSources);
    }

    [Fact]
    public async Task AddAsync_SecondRecordForTheSameProduct_ViolatesTheUniqueIndex()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var product = await SeedProductAsync(seedContext);
        seedContext.ProductTraceabilities.Add(MakeTraceability(product));
        await seedContext.SaveChangesAsync(Ct);

        var repository = new TraceabilityRepository(scope.NewContext());
        await repository.AddAsync(MakeTraceability(product), Ct);

        await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync(Ct));
    }

    [Fact]
    public async Task Remove_ThenSaveChangesAsync_DeletesTheRecordAndItsChildRows()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var product = await SeedProductAsync(seedContext);
        var traceability = MakeTraceability(product);
        seedContext.ProductTraceabilities.Add(traceability);
        await seedContext.SaveChangesAsync(Ct);

        var writeContext = scope.NewContext();
        var repository = new TraceabilityRepository(writeContext);
        var loaded = await repository.GetByIdAsync(traceability.Id, Ct);
        repository.Remove(loaded!);
        await repository.SaveChangesAsync(Ct);

        Assert.Null(await new TraceabilityRepository(scope.NewContext()).GetByIdAsync(traceability.Id, Ct));
        Assert.False(await scope.NewContext().MaterialSources.AnyAsync(m => m.ProductTraceabilityId == traceability.Id, Ct));
    }
}
