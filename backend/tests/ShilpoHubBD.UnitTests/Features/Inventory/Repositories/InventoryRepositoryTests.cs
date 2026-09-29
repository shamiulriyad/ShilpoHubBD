using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Inventory;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Inventory.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Inventory")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class InventoryRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public InventoryRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<(Category Category, District District, User Producer, Product Product)> SeedProductAsync(ShilpoHubDbContext context)
    {
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category " + Guid.NewGuid().ToString("N")[..8], Slug = Guid.NewGuid().ToString("N") };
        var district = await context.Districts.FirstAsync(Ct);
        var producer = TestUsers.Create();
        var product = new Product
        {
            Id = Guid.NewGuid(), Name = "Jamdani Saree", Slug = Guid.NewGuid().ToString("N"), Price = 1000, Stock = 10,
            CategoryId = category.Id, Category = category, DistrictId = district.Id, District = district,
            ProducerId = producer.Id, Producer = producer, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        context.Categories.Add(category);
        context.Users.Add(producer);
        context.Products.Add(product);
        await context.SaveChangesAsync(Ct);
        return (category, district, producer, product);
    }

    private static InventoryTransaction MakeTransaction(Product product, User createdBy, DateTime? createdAt = null, ProductVariant? variant = null) => new()
    {
        Id = Guid.NewGuid(), ProductId = product.Id, ProductVariantId = variant?.Id, ChangeAmount = 5, Reason = "x",
        PreviousStock = 10, NewStock = 15, CreatedByUserId = createdBy.Id, CreatedAt = createdAt ?? DateTime.UtcNow,
    };

    [Fact]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsTheTransaction()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (_, _, producer, product) = await SeedProductAsync(context);
        var transaction = MakeTransaction(product, producer);
        var repository = new InventoryRepository(db.NewContext());

        await repository.AddAsync(transaction, Ct);
        await repository.SaveChangesAsync(Ct);

        Assert.True(await db.NewContext().InventoryTransactions.AnyAsync(t => t.Id == transaction.Id, Ct));
    }

    [Fact]
    public async Task GetByProductAsync_ReturnsNewestFirstWithVariantAndCreatorLoaded()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (_, _, producer, product) = await SeedProductAsync(context);
        var variant = new ProductVariant { Id = Guid.NewGuid(), Name = "Large", ProductId = product.Id, Stock = 5 };
        context.ProductVariants.Add(variant);
        var older = MakeTransaction(product, producer, DateTime.UtcNow.AddDays(-1));
        var newer = MakeTransaction(product, producer, DateTime.UtcNow, variant);
        context.InventoryTransactions.AddRange(older, newer);
        await context.SaveChangesAsync(Ct);

        var transactions = await new InventoryRepository(db.NewContext()).GetByProductAsync(product.Id, Ct);

        Assert.Equal(new[] { newer.Id, older.Id }, transactions.Select(t => t.Id));
        Assert.Equal("Large", transactions[0].ProductVariant!.Name);
        Assert.Equal(producer.Id, transactions[0].CreatedBy.Id);
    }

    [Fact]
    public async Task GetByProductAsync_UnrelatedProductsTransactions_AreExcluded()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (_, _, producer, product) = await SeedProductAsync(context);
        var (_, _, otherProducer, otherProduct) = await SeedProductAsync(context);
        context.InventoryTransactions.AddRange(MakeTransaction(product, producer), MakeTransaction(otherProduct, otherProducer));
        await context.SaveChangesAsync(Ct);

        var transactions = await new InventoryRepository(db.NewContext()).GetByProductAsync(product.Id, Ct);

        Assert.All(transactions, t => Assert.Equal(product.Id, t.ProductId));
    }

    [Fact]
    public async Task GetByProductAsync_NoTransactions_ReturnsEmpty()
    {
        await using var db = await _database.BeginAsync();
        var (_, _, _, product) = await SeedProductAsync(db.NewContext());

        Assert.Empty(await new InventoryRepository(db.NewContext()).GetByProductAsync(product.Id, Ct));
    }
}
